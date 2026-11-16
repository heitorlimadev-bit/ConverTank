using Microsoft.AspNetCore.Mvc;
using ConverTank.Data;
using ConverTank.Models;
using Microsoft.EntityFrameworkCore;

namespace ConverTank.Controllers
{
    /// <summary>
    /// Tanques instalados em um posto (relação N:N Posto × modelo de Tanque).
    /// O mesmo modelo pode ser instalado várias vezes no mesmo posto.
    /// </summary>
    public class PostoTanquesController : Controller
    {
        private readonly ConteudoBanco context;

        public PostoTanquesController(ConteudoBanco context)
        {
            this.context = context;
        }

        private Usuario? UsuarioLogado()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            return usuarioId == null ? null : context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
        }

        private void CarregarListas()
        {
            ViewBag.Modelos = context.Tanques
                .Where(t => t.Status)
                .Include(t => t.Fabricante)
                .OrderBy(t => t.Volume).ThenBy(t => t.Nome)
                .ToList();
            ViewBag.Combustiveis = context.Combustiveis
                .Where(c => c.Status == true)
                .Include(c => c.Fornecedor)
                .OrderBy(c => c.Nome)
                .ToList();
        }

        /// <summary>Lista os tanques instalados no posto (id = PostoId).</summary>
        public IActionResult Index(int id)
        {
            var usuario = UsuarioLogado();
            if (usuario == null)
            {
                ViewBag.Erro = "Faça Login para acessar as funções";
                return RedirectToAction("Login", "Auth");
            }

            var posto = context.Postos.FirstOrDefault(p => p.Id == id);
            if (posto == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            var instalados = context.PostosTanques
                .Where(pt => pt.PostoId == id && pt.Status)
                .Include(pt => pt.Tanque).ThenInclude(t => t.Fabricante)
                .Include(pt => pt.Combustivel).ThenInclude(c => c.Fornecedor)
                .OrderBy(pt => pt.Id)
                .ToList();

            ViewBag.Posto = posto;
            ViewBag.PostoId = id;
            ViewBag.Administrador = usuario.Administrador;
            return View(instalados);
        }

        public IActionResult Adicionar(int postoId)
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var posto = context.Postos.FirstOrDefault(p => p.Id == postoId);
            if (posto == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            CarregarListas();
            ViewBag.Posto = posto;
            ViewBag.PostoId = postoId;
            return View();
        }

        /// <summary>Instala um modelo no posto. "quantidade" &gt; 1 cria várias instalações do mesmo modelo.</summary>
        [HttpPost]
        public IActionResult Adicionar(PostoTanque postoTanque, int quantidade = 1)
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            quantidade = Math.Clamp(quantidade, 1, 20);
            var identificacao = string.IsNullOrWhiteSpace(postoTanque.Identificacao) ? null : postoTanque.Identificacao.Trim();

            for (int i = 1; i <= quantidade; i++)
            {
                context.PostosTanques.Add(new PostoTanque
                {
                    PostoId = postoTanque.PostoId,
                    TanqueId = postoTanque.TanqueId,
                    CombustivelId = postoTanque.CombustivelId,
                    Identificacao = identificacao == null ? null : (quantidade > 1 ? $"{identificacao} {i}" : identificacao),
                    Status = true
                });
            }

            context.SaveChanges();
            return RedirectToAction("Index", new { id = postoTanque.PostoId });
        }

        public IActionResult Editar(int id)
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var postoTanque = context.PostosTanques
                .Include(pt => pt.Posto)
                .FirstOrDefault(pt => pt.Id == id && pt.Status);
            if (postoTanque == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            CarregarListas();
            ViewBag.Posto = postoTanque.Posto;
            ViewBag.PostoId = postoTanque.PostoId;
            return View(postoTanque);
        }

        [HttpPost]
        public IActionResult Editar(PostoTanque postoTanque)
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var banco = context.PostosTanques.FirstOrDefault(pt => pt.Id == postoTanque.Id);
            if (banco == null)
            {
                return RedirectToAction("Index", "Postos");
            }

            banco.TanqueId = postoTanque.TanqueId;
            banco.CombustivelId = postoTanque.CombustivelId;
            banco.Identificacao = string.IsNullOrWhiteSpace(postoTanque.Identificacao) ? null : postoTanque.Identificacao.Trim();
            context.SaveChanges();

            return RedirectToAction("Index", new { id = banco.PostoId });
        }

        /// <summary>Remove o tanque do posto (exclusão lógica; o histórico de medições é mantido).</summary>
        public IActionResult Apagar(int id)
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var postoTanque = context.PostosTanques.Find(id);
            if (postoTanque != null)
            {
                postoTanque.Status = false;
                context.SaveChanges();
                return RedirectToAction("Index", new { id = postoTanque.PostoId });
            }

            return RedirectToAction("Index", "Postos");
        }
    }
}
