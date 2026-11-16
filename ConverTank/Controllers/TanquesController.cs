using Microsoft.AspNetCore.Mvc;
using ConverTank.Data;
using ConverTank.Models;
using Microsoft.EntityFrameworkCore;

namespace ConverTank.Controllers
{
    /// <summary>
    /// Cadastro dos MODELOS de tanque (aba própria). A instalação de um modelo em um posto
    /// é feita em <see cref="PostoTanquesController"/>.
    /// </summary>
    public class TanquesController : Controller
    {
        private readonly ConteudoBanco context;

        public TanquesController(ConteudoBanco context)
        {
            this.context = context;
        }

        private Usuario? UsuarioLogado()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            return usuarioId == null ? null : context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
        }

        public IActionResult Index()
        {
            var usuario = UsuarioLogado();
            if (usuario == null)
            {
                ViewBag.Erro = "Faça Login para acessar as funções";
                return RedirectToAction("Login", "Auth");
            }

            var tanques = context.Tanques
                .Where(t => t.Status)
                .Include(t => t.Fabricante)
                .Include(t => t.PostoTanques.Where(pt => pt.Status))
                    .ThenInclude(pt => pt.Posto)
                .AsSplitQuery()
                .OrderBy(t => t.Volume).ThenBy(t => t.Nome)
                .ToList();

            ViewBag.Administrador = usuario.Administrador;
            return View(tanques);
        }

        public IActionResult Adicionar()
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Fabricantes = context.Fabricantes.Where(f => f.Status == true).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Adicionar(Tanque tanque, double? Diametro)
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (Diametro.HasValue && Diametro.Value > 0)
            {
                tanque.Raio = Diametro.Value / 2.0;
            }

            tanque.Nome = string.IsNullOrWhiteSpace(tanque.Nome) ? null : tanque.Nome.Trim();
            tanque.Status = true;
            context.Tanques.Add(tanque);
            context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var tanque = context.Tanques.FirstOrDefault(t => t.Id == id && t.Status);
            if (tanque == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Fabricantes = context.Fabricantes.Where(f => f.Status == true).ToList();
            ViewBag.Instalacoes = context.PostosTanques.Count(pt => pt.TanqueId == id && pt.Status);
            return View(tanque);
        }

        [HttpPost]
        public IActionResult Editar(Tanque tanque, double? Diametro)
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var tanqueBanco = context.Tanques.FirstOrDefault(t => t.Id == tanque.Id);
            if (tanqueBanco == null)
            {
                return RedirectToAction("Index");
            }

            tanqueBanco.Nome = string.IsNullOrWhiteSpace(tanque.Nome) ? null : tanque.Nome.Trim();
            tanqueBanco.FabricanteId = tanque.FabricanteId;
            tanqueBanco.Volume = tanque.Volume;
            tanqueBanco.Comprimento = tanque.Comprimento;

            if (Diametro.HasValue && Diametro.Value > 0)
            {
                tanqueBanco.Raio = Diametro.Value / 2.0;
            }
            else if (tanque.Raio > 0)
            {
                tanqueBanco.Raio = tanque.Raio;
            }

            context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Apagar(int id)
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var tanque = context.Tanques.Find(id);
            if (tanque != null)
            {
                int instalacoes = context.PostosTanques.Count(pt => pt.TanqueId == id && pt.Status);
                if (instalacoes > 0)
                {
                    TempData["Erro"] = $"Este modelo está instalado em {instalacoes} tanque(s) de postos. Remova-o dos postos antes de apagar.";
                    return RedirectToAction("Index");
                }

                tanque.Status = false;
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
