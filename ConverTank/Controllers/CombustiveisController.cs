using ConverTank.Data;
using ConverTank.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConverTank.Controllers
{
    public class CombustiveisController : Controller
    {

        private ConteudoBanco context;

        public CombustiveisController(ConteudoBanco context) 
        { 
            this.context = context;
        }
        public IActionResult Index()
        {
            var combustiveis = context.Combustiveis.ToList();
            return View(combustiveis);
        }
        public IActionResult Adicionar()
        { 
        
            return View();
        
        }
        [HttpPost]
        public IActionResult Adicionar(Combustivel combustivel)
        {
            context.Combustiveis.Add(combustivel);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Editar(int Id)
        {
            var combustível = context.Combustiveis.Find(Id);


            return View(combustível);
        }
        [HttpPost]
        public IActionResult Editar(Combustivel combustivel)
        {
            context.Combustiveis.Update(combustivel);
            context.SaveChanges();
            return RedirectToAction("Index");

        }
        [HttpPost]
        public IActionResult Apagar(int Id)
        {
            var combustível = context.Combustiveis.Find(Id);
            context.Combustiveis.Remove(combustível);
            context.SaveChanges();
            
            return RedirectToAction("Index");
        }
        
    }
}
