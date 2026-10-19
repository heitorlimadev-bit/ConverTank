using Microsoft.AspNetCore.Mvc;
using ConverTank.Data;
using ConverTank.Models;
using Microsoft.EntityFrameworkCore;

namespace ConverTank.Controllers
{
    public class TanquesController : Controller
    {   
        private readonly ConteudoBanco context;

        public TanquesController(ConteudoBanco context)
        {
            this.context = context;
        }

        public IActionResult Index(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                ViewBag.Erro = "Faça Login para acessar as funções";
                return RedirectToAction("Login", "Auth");
            }

            var tanques = context.Tanques
                .Where(t => t.Status == true && t.PostoId == id)
                .Include(f => f.Fabricante)
                .Include(t => t.Combustivel)
                .ToList();

            var posto = context.Postos.FirstOrDefault(p => p.Id == id);
            ViewBag.Posto = posto;
            ViewBag.PostoId = id;

            return View(tanques);
        }

        public IActionResult Adicionar(int postoId)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Combustiveis = context.Combustiveis.Where(c => c.Status == true).Include(c => c.Fornecedor).ToList();
            ViewBag.Fabricantes = context.Fabricantes.Where(f => f.Status == true).ToList();
            ViewBag.PostoId = postoId;

            return View();
        }

        [HttpPost]
        public IActionResult Adicionar(Tanque tanque, double? Diametro)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (Diametro.HasValue && Diametro.Value > 0)
            {
                tanque.Raio = Diametro.Value / 2.0;
            }

            tanque.Status = true;
            context.Tanques.Add(tanque);
            context.SaveChanges();

            return RedirectToAction("Index", new { id = tanque.PostoId });
        }

        public IActionResult Editar(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var tanque = context.Tanques.FirstOrDefault(t => t.Id == id && t.Status);
            if (tanque == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            ViewBag.Combustiveis = context.Combustiveis.Where(c => c.Status == true).Include(c => c.Fornecedor).ToList();
            ViewBag.Fabricantes = context.Fabricantes.Where(f => f.Status == true).ToList();
            ViewBag.PostoId = tanque.PostoId;

            return View(tanque);
        }

        [HttpPost]
        public IActionResult Editar(Tanque tanque, double? Diametro)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var tanqueBanco = context.Tanques.FirstOrDefault(t => t.Id == tanque.Id);
            if (tanqueBanco == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            tanqueBanco.FabricanteId = tanque.FabricanteId;
            tanqueBanco.CombustivelId = tanque.CombustivelId;
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

            return RedirectToAction("Index", new { id = tanqueBanco.PostoId });
        }

        public IActionResult Apagar(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var tanque = context.Tanques.Find(id);
            if (tanque != null)
            {
                tanque.Status = false;
                context.Update(tanque);
                context.SaveChanges();
                return RedirectToAction("Index", new { id = tanque.PostoId });
            }

            return RedirectToAction("Index", "Postos");
        }
    }
}
