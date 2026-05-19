using Microsoft.AspNetCore.Mvc;
using ConverTank.Data;
using ConverTank.Models;


namespace ConverTank.Controllers
{
    public class TanquesController : Controller
    {   
        private ConteudoBanco context;

        public TanquesController(ConteudoBanco context)
        {
            this.context = context;
        }

        public IActionResult Index(int id)
        {
            var tanques = context.Tanques.Where(t => t.PostoId == id).ToList();

            return View(tanques);
        }

        public IActionResult Adicionar(int postoId)
        {

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
    }
}
