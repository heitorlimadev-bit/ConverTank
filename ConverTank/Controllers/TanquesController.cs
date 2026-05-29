using Microsoft.AspNetCore.Mvc;
using ConverTank.Data;
using ConverTank.Models;
using Microsoft.EntityFrameworkCore;


namespace ConverTank.Controllers
{
    public class TanquesController : Controller
    {   
        private ConteudoBanco context;

        public TanquesController (ConteudoBanco context)
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

            var tanques = context.Tanques.Include(f => f.Fabricante).Where(t => t.PostoId == id).ToList();

            ViewBag.PostoId = id;

            return View(tanques);
        }

        public IActionResult Adicionar(int postoId)
        {
            ViewBag.Combustiveis = context.Combustiveis.Include(c => c.Fornecedor).ToList();
            ViewBag.Fabricantes = context.Fabricantes.ToList();
            ViewBag.PostoId = postoId;

            return View();

        }
        [HttpPost]
        public IActionResult Adicionar(Tanque tanque)
        {

            context.Tanques.Add(tanque);

            context.SaveChanges();

            return RedirectToAction("index", new {id = tanque.PostoId});

        }
        public IActionResult Editar(int id)
        {
            var tanque = context.Tanques.Find(id);
            ViewBag.Combustiveis = context.Combustiveis.Include(c => c.Fornecedor).ToList();
            ViewBag.Fabricantes = context.Fabricantes.ToList();
            ViewBag.PostoId = tanque.PostoId;
            return View(tanque);
        
        }
        [HttpPost]
        public IActionResult Editar(Tanque tanque)
        {

            context.Tanques.Update(tanque);

            context.SaveChanges();

            return RedirectToAction("index", new { id = tanque.PostoId });

        }
    }
}
