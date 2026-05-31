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
                return RedirectToAction("Login", "Auth");
            }

            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (usuario.Administrador == true )
            {
                var todosPostos = context.Postos.Where(p => p.Status == true).ToList();

                return View(todosPostos);
            }

            var postosPermitidos = context.UsuariosPostos.Where(up => up.UsuarioId == usuarioId).Select(up => up.Posto).Where(p => p.Status == true).ToList();

            return View(postosPermitidos);
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
        public IActionResult Adicionar(Posto posto) 
        {

            context.Postos.Add(posto);

            context.SaveChanges();

            return RedirectToAction("index");

        }
        public IActionResult Editar(int id) 
        {


            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario.Administrador == true)
            {

                var posto = context.Postos.Find(id);

                return View(posto);

            }
            else { return RedirectToAction("Login", "Auth"); }



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

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario.Administrador == true)
            {

                var posto = context.Postos.Find(Id);
                posto.Status = false;
                context.Postos.Update(posto);
                context.SaveChanges();
                return RedirectToAction("Index");

            }
            else { return RedirectToAction("Login", "Auth"); }




        }
    }
}
