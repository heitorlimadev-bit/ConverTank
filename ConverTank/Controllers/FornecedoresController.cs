using Microsoft.AspNetCore.Mvc;
using ConverTank.Data;
using ConverTank.Models;
namespace ConverTank.Controllers
{
    public class FornecedoresController : Controller
    {

        private ConteudoBanco context;

        public FornecedoresController(ConteudoBanco context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            var fornecedores = context.Fornecedores.ToList();
            return View(fornecedores);
        }
        public IActionResult Adicionar()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Adicionar(Fornecedor fornecedor) 
        {
            context.Fornecedores.Add(fornecedor);
            context.SaveChanges();
            return RedirectToAction("Index");
        
        }
        public IActionResult Editar(int Id)
        {
            var fornecedor = context.Fornecedores.Find(Id);
            return View(fornecedor); 

        }
        [HttpPost]
        public IActionResult Editar(Fornecedor fornecedor)
        {

            context.Fornecedores.Update(fornecedor);
            return RedirectToAction("Index");

        }
        [HttpPost]
        public IActionResult Apagar(int Id)
        {
            var fornecedor = context.Fornecedores.Find(Id);
            context.Fornecedores.Remove(fornecedor);
            context.SaveChanges();
            return RedirectToAction("Index");

        }
    }
}
