using ConverTank.Data;
using Microsoft.AspNetCore.Mvc;

namespace ConverTank.Controllers
{
    public class UsuariosController : Controller
    {
        private ConteudoBanco context;

        public UsuariosController(ConteudoBanco context)
        {
            this.context = context;
        }

        public IActionResult PermissaoUsuario(int usuarioId)
        {
            bool admin = bool.Parse(HttpContext.Session.GetString("Administrador"));

            if (admin == false) 
            {
                return RedirectToAction("AcessoNegado", "Auth");
            
            }

            ViewBag.Postos = context.Postos.ToList();
            ViewBag.UsuarioId = usuarioId;
            
            return View();
        }
    }
}
