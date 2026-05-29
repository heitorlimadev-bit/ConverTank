using ConverTank.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ConverTank.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                ViewBag.Erro = "Faça Login para acessar as funções";
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

    }
}
