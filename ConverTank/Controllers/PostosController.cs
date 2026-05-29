using Microsoft.AspNetCore.Mvc;
using ConverTank.Data;
using ConverTank.Models;
using Microsoft.EntityFrameworkCore;

namespace ConverTank.Controllers
{
    public class PostosController : Controller
    {
        private ConteudoBanco context;

        public PostosController (ConteudoBanco context) { 
        
            this.context = context;
        }

        public IActionResult Index()
        {

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                ViewBag.Erro = "Faça Login para acessar as funções";
                return RedirectToAction("Login", "Auth");
            }

            var postos = context.Postos.ToList();
            return View(postos);

        }
        public IActionResult Adicionar()
        {

            return View();

        }
        [HttpPost]
        public IActionResult Adicionar(Posto posto) 
        {

            context.Postos.Add(posto);

            context.SaveChanges();

            return RedirectToAction("index");

        }
        public IActionResult Editar(int id) 
        {
            var posto = context.Postos.Find(id);

                return View(posto);
        }
        [HttpPost]
        public IActionResult Editar(Posto posto)
        {

            context.Postos.Update(posto);
            context.SaveChanges();
            return RedirectToAction("Index");

        }
        public IActionResult Apagar(int Id)
        {
            var posto = context.Postos.Find(Id);
            context.Postos.Remove(posto);
            context.SaveChanges();
            return RedirectToAction("Index");

        }
    }
}
