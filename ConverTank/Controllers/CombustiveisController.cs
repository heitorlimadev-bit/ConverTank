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
            var combustiveis = context.Combustiveis.Where(c => c.Status == true).Include(c => c.Fornecedor).ToList();
            return View(combustiveis);
        }
        public IActionResult Adicionar()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario.Administrador == true)
            {

                ViewBag.Fornecedores = context.Fornecedores.Where(f => f.Status == true).ToList();
                return View();
            }
            else { return RedirectToAction("Login", "Auth"); }

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

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario.Administrador == true)
            {


                var combustível = context.Combustiveis.Find(Id);
                ViewBag.Fornecedores = context.Fornecedores.Where(f => f.Status == true).ToList();

                return View(combustível);

            }
            else { return RedirectToAction("Login", "Auth"); }



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

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario.Administrador == true)
            {


                var combustivel = context.Combustiveis.Find(Id);

                combustivel.Status = false;
                context.Combustiveis.Update(combustivel);
                context.SaveChanges();

                return RedirectToAction("Index");


            }
            else { return RedirectToAction("Login", "Auth"); }







        }
        
    }
}
