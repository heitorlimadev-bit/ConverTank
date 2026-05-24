using Microsoft.AspNetCore.Mvc;
using ConverTank.Data;
using ConverTank.Models;
namespace ConverTank.Controllers
{
    public class FabricantesController : Controller
    {

        private ConteudoBanco context;

        public FabricantesController(ConteudoBanco context) 
        { 
        
            this.context = context;

        }
        public IActionResult Index()
        {
            var fabricantes = context.Fabricantes;

            return View(fabricantes);
        }
        public IActionResult Adicionar()
        {

            return View();

        }
        [HttpPost]
        public IActionResult Adicionar(Fabricante fabricante)
        {
            context.Fabricantes.Add(fabricante);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Editar(int Id)
        {

            var fabricante = context.Fabricantes.Find(Id);
            return View(fabricante);

        }
        [HttpPost]
        public IActionResult Editar(Fabricante fabricante)
        { 
        
            context.Fabricantes.Update(fabricante);
            context.SaveChanges();
            return RedirectToAction("Index");
        
        }
        [HttpPost]
        public IActionResult Apagar(int id)
        {

            var fabricante = context.Fabricantes.Find(id);
            context.Fabricantes.Remove(fabricante);
            context.SaveChanges();
            return RedirectToAction("Index");

        }


    }
}
