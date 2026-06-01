using ConverTank.Data;
using ConverTank.Models;
using ConverTank.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConverTank.Controllers
{
    public class MedicoesController : Controller
    {

        private ConteudoBanco context;

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

            var medicoes = context.Medicoes.Where(t => t.TanqueId == Id && t.Status == true).ToList();


            ViewBag.TanqueId = Id;

            return View(medicoes);
        }

        public IActionResult Adicionar(int TanqueId)
        {

            ViewBag.TanqueId = TanqueId;

            return View();

        }
        [HttpPost]
        public IActionResult Adicionar(Medicao medicao)
        {

            var tanque = context.Tanques.Find(medicao.TanqueId);

            medicao.Volume = TanqueService.CalcularVolume(tanque.Raio, tanque.Comprimento, medicao.Altura);

            medicao.DataMedicao = DateTime.Now;

            context.Medicoes.Add(medicao);

            context.SaveChanges();

            return RedirectToAction("index", new { id = medicao.TanqueId });

        }
        public IActionResult Editar(int id)
        {

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario.Administrador == true)
            {

                var medicao = context.Medicoes.Find(id);
                ViewBag.TanqueId = medicao.TanqueId;
                return View(medicao);

            }
            else { return RedirectToAction("Login", "Auth"); }



        }
        [HttpPost]
        public IActionResult Editar(Medicao medicao)
        {
            var tanque = context.Tanques.Find(medicao.TanqueId);
            context.Medicoes.Update(medicao);
            medicao.Volume = TanqueService.CalcularVolume(tanque.Raio, tanque.Comprimento, medicao.Altura);
            context.SaveChanges();

            return RedirectToAction("index", new { id = medicao.TanqueId });

        }

        public IActionResult Apagar(int id)
        {

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario.Administrador == true)
            {

                var medicao = context.Medicoes.Find(id);

                medicao.Status = false;
                context.Update(medicao);
                context.SaveChanges();

                return RedirectToAction("index", new { id = medicao.TanqueId });

            }
            else { return RedirectToAction("Login", "Auth"); }



        }
    }
}

    

