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
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                ViewBag.Erro = "Faça Login para acessar as funções";
                return RedirectToAction("Login", "Auth");
            }
            var fabricantes = context.Fabricantes.Where(f => f.Status == true).ToList();

            return View(fabricantes);
        }
        public IActionResult Adicionar()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario.Administrador == true)
            {

                return View();

            }
            else { return RedirectToAction("Login", "Auth"); }


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
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario.Administrador == true)
            {

                var fabricante = context.Fabricantes.Find(Id);
                return View(fabricante);

            }
            else { return RedirectToAction("Login", "Auth"); }


        }
        [HttpPost]
        public IActionResult Editar(Fabricante fabricante)
        { 
        
            context.Fabricantes.Update(fabricante);
            context.SaveChanges();
            return RedirectToAction("Index");
        
        }
        public IActionResult Apagar(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario.Administrador == true)
            {

                var fabricante = context.Fabricantes.Find(id);
                fabricante.Status = false;
                context.Fabricantes.Update(fabricante);
                context.SaveChanges();
                return RedirectToAction("Index");

            }
            else { return RedirectToAction("Login", "Auth"); }


        }


    }
}
