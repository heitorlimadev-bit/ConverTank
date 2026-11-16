using ConverTank.Data;
using ConverTank.Models;
using ConverTank.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConverTank.Controllers
{
    public class MedicoesController : Controller
    {
        private readonly ConteudoBanco context;

        public MedicoesController(ConteudoBanco context)
        {
            this.context = context;
        }

        public IActionResult Index(int Id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                ViewBag.Erro = "Faça Login para acessar as funções";
                return RedirectToAction("Login", "Auth");
            }

            // Id = PostoTanque (tanque instalado no posto)
            var postoTanque = context.PostosTanques
                .Include(pt => pt.Posto)
                .Include(pt => pt.Combustivel)
                .Include(pt => pt.Tanque).ThenInclude(t => t.Fabricante)
                .FirstOrDefault(pt => pt.Id == Id);

            if (postoTanque == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            var medicoes = context.Medicoes
                .Where(m => m.PostoTanqueId == Id && m.Status == true)
                .OrderByDescending(m => m.DataMedicao)
                .ToList();

            ViewBag.PostoTanqueId = Id;
            ViewBag.PostoTanque = postoTanque;
            ViewBag.PostoId = postoTanque.PostoId;

            return View(medicoes);
        }

        public IActionResult Adicionar(int PostoTanqueId)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                ViewBag.Erro = "Faça Login para acessar as funções";
                return RedirectToAction("Login", "Auth");
            }

            var postoTanque = context.PostosTanques
                .Include(pt => pt.Posto)
                .Include(pt => pt.Combustivel)
                .Include(pt => pt.Tanque).ThenInclude(t => t.Fabricante)
                .FirstOrDefault(pt => pt.Id == PostoTanqueId);

            if (postoTanque == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            ViewBag.PostoTanqueId = PostoTanqueId;
            ViewBag.PostoTanque = postoTanque;

            return View();
        }

        [HttpPost]
        public IActionResult Adicionar(Medicao medicao)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                ViewBag.Erro = "Faça Login para acessar as funções";
                return RedirectToAction("Login", "Auth");
            }

            var postoTanque = context.PostosTanques
                .Include(pt => pt.Tanque)
                .FirstOrDefault(pt => pt.Id == medicao.PostoTanqueId);

            if (postoTanque == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            var tanque = postoTanque.Tanque;
            medicao.Volume = TanqueService.CalcularVolume(tanque.Raio, tanque.Comprimento, medicao.Altura);
            medicao.DataMedicao = DateTime.Now;
            medicao.Status = true;

            context.Medicoes.Add(medicao);
            context.SaveChanges();

            return RedirectToAction("Index", new { id = medicao.PostoTanqueId });
        }

        public IActionResult Editar(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var medicao = context.Medicoes
                .Include(m => m.PostoTanque).ThenInclude(pt => pt.Tanque)
                .Include(m => m.PostoTanque).ThenInclude(pt => pt.Combustivel)
                .Include(m => m.PostoTanque).ThenInclude(pt => pt.Posto)
                .FirstOrDefault(m => m.Id == id && m.Status == true);

            if (medicao == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            ViewBag.PostoTanqueId = medicao.PostoTanqueId;
            ViewBag.PostoTanque = medicao.PostoTanque;

            return View(medicao);
        }

        [HttpPost]
        public IActionResult Editar(Medicao medicao)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var medicaoBanco = context.Medicoes.FirstOrDefault(m => m.Id == medicao.Id);
            if (medicaoBanco == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            var tanque = context.PostosTanques
                .Where(pt => pt.Id == medicaoBanco.PostoTanqueId)
                .Select(pt => pt.Tanque)
                .FirstOrDefault();
            if (tanque == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            medicaoBanco.Altura = medicao.Altura;
            medicaoBanco.Volume = TanqueService.CalcularVolume(tanque.Raio, tanque.Comprimento, medicao.Altura);
            context.SaveChanges();

            return RedirectToAction("Index", new { id = medicaoBanco.PostoTanqueId });
        }

        public IActionResult Apagar(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var medicao = context.Medicoes.Find(id);
            if (medicao != null)
            {
                medicao.Status = false;
                context.Update(medicao);
                context.SaveChanges();
                return RedirectToAction("Index", new { id = medicao.PostoTanqueId });
            }

            return RedirectToAction("Index", "Postos");
        }
    }
}
