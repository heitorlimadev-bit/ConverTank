using ConverTank.Data;
using ConverTank.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            if (usuarioId == null)
            {
                ViewBag.Erro = "Faça Login para acessar as funções";
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                bool admin = bool.Parse(HttpContext.Session.GetString("Administrador"));

                if (admin == false)
                {
                    return RedirectToAction("Login", "Auth");

                }
            }


            ViewBag.Postos = context.Postos.Where(p => p.Status == true).ToList();
            ViewBag.UsuarioId = usuarioId;

            var usuarios = context.Usuarios.ToList();

            return View(usuarios);
        }
        public IActionResult PermissoesUsuario(int id)
        {
            bool admin = bool.Parse(HttpContext.Session.GetString("Administrador"));

            if (admin == false)
            {
                return RedirectToAction("Login", "Auth");

            }
            var usuario = context.Usuarios
                .Include(u => u.UsuarioPostos)
                .FirstOrDefault(u => u.Id == id);

            if (usuario == null)
                return NotFound();

            var vm = new PermissaoUsuarioVM
            {
                UsuarioId = usuario.Id,
                NomeUsuario = usuario.Nome,
                Administrador = usuario.Administrador,
                TodosPostos = context.Postos.ToList(),
                PostosPermitidos = usuario.UsuarioPostos
                    .Select(up => up.PostoId)
                    .ToList()
            };

            return View(vm);
        }
        [HttpPost]
        public IActionResult SalvarPermissoes(PermissaoUsuarioVM vm)
        {
            var usuario = context.Usuarios
                .Include(u => u.UsuarioPostos)
                .FirstOrDefault(u => u.Id == vm.UsuarioId);

            if (usuario == null)
                return NotFound();

            usuario.Administrador = vm.Administrador;

            context.UsuariosPostos.RemoveRange(usuario.UsuarioPostos);

            if (vm.PostosPermitidos != null)
            {
                foreach (var postoId in vm.PostosPermitidos)
                {
                    context.UsuariosPostos.Add(
                        new UsuarioPosto
                        {
                            UsuarioId = usuario.Id,
                            PostoId = postoId
                        });
                }
            }

            context.SaveChanges();

            return RedirectToAction("PermissaoUsuario","Usuarios");
        }
    }
}
