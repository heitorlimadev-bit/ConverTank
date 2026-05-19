using ConverTank.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ConverTank.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}
