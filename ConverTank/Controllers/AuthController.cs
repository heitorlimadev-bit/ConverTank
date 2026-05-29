using ConverTank.Data;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConverTank.Models;

namespace ConverTank.Controllers
{
    public class AuthController : Controller
    {

        private ConteudoBanco context;

        public AuthController(ConteudoBanco context)
        {
            this.context = context;
        }

        public IActionResult Login()
        {

            
            return View();

        }
        [HttpPost]
        public IActionResult Login(LoginViewModel dados)
        {  
            
            Usuario usuario = context.Usuarios.FirstOrDefault(u => u.Login == dados.Login && u.Senha == dados.Senha);


            if (usuario == null) 
            {
                ViewBag.Erro = "Login Invalido"; 
                return View();
            }

            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("Administrador", usuario.Administrador.ToString());

            return RedirectToAction("Index", "Home");
        }
        public IActionResult NovoAcesso()
        {

            return View();
        
        }
        [HttpPost]
        public IActionResult NovoAcesso(NovoAcessoViewModel dados)
        {

            if (dados.Senha != dados.ConfirmarSenha)
            {
                ViewBag.Erro = "Senhas não conferem!";

                return RedirectToAction("Auth", "NovoAcesso");

            }


            Usuario novoUsuario = new Usuario();

            novoUsuario.Nome = dados.Nome;
            novoUsuario.Login = dados.Login;
            novoUsuario.Senha = dados.Senha;

            context.Usuarios.Add(novoUsuario);
            context.SaveChanges();

            return RedirectToAction("Login", "Auth");

        }
    }
}
