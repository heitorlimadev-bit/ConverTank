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
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                ViewBag.Erro = "Faça Login para acessar as funções";
                return RedirectToAction("Login", "Auth");
            }
            var fornecedores = context.Fornecedores.Where(f => f.Status == true).ToList();
            return View(fornecedores);
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
        public IActionResult Adicionar(Fornecedor fornecedor) 
        {
            context.Fornecedores.Add(fornecedor);
            context.SaveChanges();
            return RedirectToAction("Index");
        
        }
        public IActionResult Editar(int Id)
        {

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario.Administrador == true)
            {

                var fornecedor = context.Fornecedores.Find(Id);
                return View(fornecedor);

            }
            else { return RedirectToAction("Login", "Auth"); }



        }
        [HttpPost]
        public IActionResult Editar(Fornecedor fornecedor)
        {

            context.Fornecedores.Update(fornecedor);
            context.SaveChanges();
            return RedirectToAction("Index");

        }
        public IActionResult Apagar(int Id)
        {

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario.Administrador == true)
            {

                var fornecedor = context.Fornecedores.Find(Id);
                var combustivelFornecedor = context.Combustiveis.Where(c => c.FornecedorId == Id);

                foreach (Combustivel c in combustivelFornecedor)
                {

                    c.Status = false;

                }

                fornecedor.Status = false;
                context.Fornecedores.Update(fornecedor);
                context.SaveChanges();
                return RedirectToAction("Index");

            }
            else { return RedirectToAction("Login", "Auth"); }



        }
    }
}
