using ConverTank.Data;
using ConverTank.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                ViewBag.Erro = "Faça Login para acessar as funções";
                return RedirectToAction("Login", "Auth");
            }
            var combustiveis = context.Combustiveis.Include(c => c.Fornecedor).ToList();
            return View(combustiveis);
        }
        public IActionResult Adicionar()
        {

            ViewBag.Fornecedores = context.Fornecedores.ToList();
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
            ViewBag.Fornecedores = context.Fornecedores.ToList();

            return View(combustível);
        }
        [HttpPost]
        public IActionResult Editar(Combustivel combustivel)
        {
            context.Combustiveis.Update(combustivel);
            context.SaveChanges();
            return RedirectToAction("Index");

        }
        public IActionResult Apagar(int Id)
        {
            var combustível = context.Combustiveis.Find(Id);
            context.Combustiveis.Remove(combustível);
            context.SaveChanges();
            
            return RedirectToAction("Index");
        }
        
    }
}
