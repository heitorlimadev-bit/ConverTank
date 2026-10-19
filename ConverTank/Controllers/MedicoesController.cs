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

            var tanque = context.Tanques
                .Include(t => t.Posto)
                .Include(t => t.Combustivel)
                .Include(t => t.Fabricante)
                .FirstOrDefault(t => t.Id == Id);

            if (tanque == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            var medicoes = context.Medicoes
                .Where(t => t.TanqueId == Id && t.Status == true)
                .OrderByDescending(m => m.DataMedicao)
                .ToList();

            ViewBag.TanqueId = Id;
            ViewBag.Tanque = tanque;
            ViewBag.PostoId = tanque.PostoId;

            return View(medicoes);
        }

        public IActionResult Adicionar(int TanqueId)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                ViewBag.Erro = "Faça Login para acessar as funções";
                return RedirectToAction("Login", "Auth");
            }

            var tanque = context.Tanques
                .Include(t => t.Combustivel)
                .Include(t => t.Fabricante)
                .FirstOrDefault(t => t.Id == TanqueId);

            if (tanque == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            ViewBag.TanqueId = TanqueId;
            ViewBag.Tanque = tanque;

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

            var tanque = context.Tanques.FirstOrDefault(t => t.Id == medicao.TanqueId);

            if (tanque == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            medicao.Volume = TanqueService.CalcularVolume(tanque.Raio, tanque.Comprimento, medicao.Altura);
            medicao.DataMedicao = DateTime.Now;
            medicao.Status = true;

            context.Medicoes.Add(medicao);
            context.SaveChanges();

            return RedirectToAction("Index", new { id = medicao.TanqueId });
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
                .Include(m => m.Tanque)
                .FirstOrDefault(m => m.Id == id && m.Status == true);

            if (medicao == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            ViewBag.TanqueId = medicao.TanqueId;
            ViewBag.Tanque = medicao.Tanque;

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

            var tanque = context.Tanques.FirstOrDefault(t => t.Id == medicaoBanco.TanqueId);
            if (tanque == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            medicaoBanco.Altura = medicao.Altura;
            medicaoBanco.Volume = TanqueService.CalcularVolume(tanque.Raio, tanque.Comprimento, medicao.Altura);
            context.SaveChanges();

            return RedirectToAction("Index", new { id = medicaoBanco.TanqueId });
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
                return RedirectToAction("Index", new { id = medicao.TanqueId });
            }

            return RedirectToAction("Index", "Postos");
        }
    }
}
