using ConverTank.Data;
using ConverTank.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConverTank.Controllers
{
    public class InventariosController : Controller
    {
        private const string MimeXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly ConteudoBanco context;
        private readonly IConfiguration configuracao;

        public InventariosController(ConteudoBanco context, IConfiguration configuracao)
        {
            this.context = context;
            this.configuracao = configuracao;
        }

        // ------------------------------------------------------------------
        //  Auxiliares
        // ------------------------------------------------------------------

        private Usuario? UsuarioLogado()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            return usuarioId == null ? null : context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
        }

        private Inventario? CarregarInventario(int id) =>
            context.Inventarios
                .Include(i => i.Itens).ThenInclude(it => it.PostoTanque).ThenInclude(pt => pt.Tanque)
                .Include(i => i.Itens).ThenInclude(it => it.Medicao)
                .AsSplitQuery()
                .FirstOrDefault(i => i.Id == id && i.Status);

        private List<Posto> PostosComTanques() =>
            context.Postos
                .Where(p => p.Status)
                .Include(p => p.PostoTanques.Where(pt => pt.Status))
                    .ThenInclude(pt => pt.Tanque)
                .Include(p => p.PostoTanques.Where(pt => pt.Status))
                    .ThenInclude(pt => pt.Combustivel)
                        .ThenInclude(c => c.Fornecedor)
                .AsSplitQuery()
                .ToList();

        private static string NomeArquivo(Inventario inventario) =>
            $"Inventario_{inventario.Ano}_T{inventario.Trimestre}.xlsx";

        // ------------------------------------------------------------------
        //  Listagem
        // ------------------------------------------------------------------

        public IActionResult Index()
        {
            var usuario = UsuarioLogado();
            if (usuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var inventarios = context.Inventarios
                .Where(i => i.Status)
                .Include(i => i.Itens)
                .OrderByDescending(i => i.Ano).ThenByDescending(i => i.Trimestre)
                .ToList();

            ViewBag.Administrador = usuario.Administrador;
            return View(inventarios);
        }

        // ------------------------------------------------------------------
        //  Novo inventário
        // ------------------------------------------------------------------

        public IActionResult Novo()
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var (ano, trimestre) = Inventario.UltimoTrimestreFechado(DateTime.Today);
            ViewBag.Ano = ano;
            ViewBag.Trimestre = trimestre;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Novo(int ano, int trimestre)
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (trimestre < 1 || trimestre > 4 || ano < 2000 || ano > 2100)
            {
                ViewBag.Erro = "Informe um ano válido e um trimestre entre 1 e 4.";
                ViewBag.Ano = ano;
                ViewBag.Trimestre = trimestre;
                return View();
            }

            var existente = context.Inventarios.FirstOrDefault(i => i.Ano == ano && i.Trimestre == trimestre && i.Status);
            if (existente != null)
            {
                TempData["Mensagem"] = $"Já existe um inventário para o {trimestre}º trimestre de {ano}.";
                return RedirectToAction("Detalhes", new { id = existente.Id });
            }

            var inventario = Inventario.Novo(ano, trimestre);
            inventario.SincronizarTanques(PostosComTanques());
            context.Inventarios.Add(inventario);
            context.SaveChanges();

            TempData["Mensagem"] = $"Inventário criado com {inventario.Itens.Count} tanques. Baixe o formulário para preencher as medições.";
            return RedirectToAction("Detalhes", new { id = inventario.Id });
        }

        // ------------------------------------------------------------------
        //  Formulário de entrada (download da planilha)
        // ------------------------------------------------------------------

        public IActionResult Formulario(int id)
        {
            var usuario = UsuarioLogado();
            if (usuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var inventario = CarregarInventario(id);
            if (inventario == null)
            {
                return RedirectToAction("Index");
            }

            // Administradores sincronizam tanques novos antes de gerar; demais apenas baixam
            byte[] arquivo = usuario.Administrador
                ? inventario.CriarFormulario(PostosComTanques())
                : inventario.CriarFormulario();

            if (usuario.Administrador)
            {
                context.SaveChanges();
            }

            return File(arquivo, MimeXlsx, NomeArquivo(inventario));
        }

        // ------------------------------------------------------------------
        //  Planilha de saída (estoque valorizado) — para inventários já prontos
        // ------------------------------------------------------------------

        public IActionResult Saida(int id)
        {
            var usuario = UsuarioLogado();
            if (usuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var inventario = CarregarInventario(id);
            if (inventario == null)
            {
                return RedirectToAction("Index");
            }

            var nomeGrupo = configuracao["Inventario:NomeGrupo"] ?? "GRUPO ITAMARATI";
            var arquivo = inventario.CriarPlanilhaSaida(nomeGrupo);
            return File(arquivo, MimeXlsx, $"INV_{inventario.DataReferencia:dd-MM-yyyy}.xlsx");
        }

        // ------------------------------------------------------------------
        //  Importação da planilha preenchida
        // ------------------------------------------------------------------

        public IActionResult Importar(int id)
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var inventario = CarregarInventario(id);
            if (inventario == null)
            {
                return RedirectToAction("Index");
            }

            return View(inventario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Importar(int id, IFormFile? planilha)
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var inventario = CarregarInventario(id);
            if (inventario == null)
            {
                return RedirectToAction("Index");
            }

            var resultado = new ResultadoImportacao();
            if (planilha == null || planilha.Length == 0)
            {
                resultado.Erros.Add("Selecione a planilha preenchida (.xlsx).");
            }
            else if (!Path.GetExtension(planilha.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                resultado.Erros.Add("O arquivo precisa estar no formato .xlsx.");
            }
            else
            {
                using var stream = planilha.OpenReadStream();
                resultado = inventario.FazerInventario(stream);

                // Só grava se não houve erro: nada é salvo pela metade
                if (resultado.Sucesso)
                {
                    context.SaveChanges();
                }
            }

            ViewBag.Resultado = resultado;
            return View(inventario);
        }

        // ------------------------------------------------------------------
        //  Preço por litro
        // ------------------------------------------------------------------

        public IActionResult Precos(int id)
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var inventario = CarregarInventario(id);
            if (inventario == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Grupos = inventario.GruposDeCombustivel();
            ViewBag.Sugestoes = PrecosDoInventarioAnterior(inventario);
            return View(inventario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Precos(int id, List<PrecoGrupoInput> grupos, List<PrecoItemInput> itens)
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var inventario = CarregarInventario(id);
            if (inventario == null)
            {
                return RedirectToAction("Index");
            }

            var erros = new List<string>();

            // 1) Preço do grupo: altera todos os tanques com mesmo combustível e fornecedor
            foreach (var grupo in grupos ?? new List<PrecoGrupoInput>())
            {
                if (Igual(grupo.Ppl, grupo.PplOriginal))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(grupo.Ppl))
                {
                    inventario.AlterarPpl(grupo.Combustivel, grupo.FornecedorId, null);
                }
                else if (Inventario.TentarLerDecimal(grupo.Ppl, out var ppl) && ppl >= 0)
                {
                    inventario.AlterarPpl(grupo.Combustivel, grupo.FornecedorId, ppl);
                }
                else
                {
                    erros.Add($"Preço inválido para {grupo.Combustivel}: \"{grupo.Ppl}\".");
                }
            }

            // 2) Exceções por tanque (somente os que foram alterados individualmente)
            foreach (var entrada in itens ?? new List<PrecoItemInput>())
            {
                if (Igual(entrada.Ppl, entrada.PplOriginal))
                {
                    continue;
                }

                var item = inventario.Itens.FirstOrDefault(i => i.Id == entrada.Id);
                if (item == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entrada.Ppl))
                {
                    item.Ppl = null;
                }
                else if (Inventario.TentarLerDecimal(entrada.Ppl, out var ppl) && ppl >= 0)
                {
                    item.Ppl = ppl;
                }
                else
                {
                    erros.Add($"Preço inválido para {item.PostoNome} {item.Rotulo}: \"{entrada.Ppl}\".");
                }
            }

            if (erros.Count > 0)
            {
                ViewBag.Erros = erros;
                ViewBag.Grupos = inventario.GruposDeCombustivel();
                ViewBag.Sugestoes = PrecosDoInventarioAnterior(inventario);
                return View(inventario);
            }

            context.SaveChanges();
            TempData["Mensagem"] = "Preços salvos.";
            return RedirectToAction("Detalhes", new { id = inventario.Id });
        }

        private static bool Igual(string? a, string? b) =>
            (a ?? string.Empty).Trim() == (b ?? string.Empty).Trim();

        /// <summary>PPL por grupo (combustível + fornecedor) do inventário imediatamente anterior.</summary>
        private Dictionary<string, decimal> PrecosDoInventarioAnterior(Inventario atual)
        {
            var anterior = context.Inventarios
                .Where(i => i.Status && i.Id != atual.Id &&
                            (i.Ano < atual.Ano || (i.Ano == atual.Ano && i.Trimestre < atual.Trimestre)))
                .OrderByDescending(i => i.Ano).ThenByDescending(i => i.Trimestre)
                .Include(i => i.Itens)
                .FirstOrDefault();

            if (anterior == null)
            {
                return new Dictionary<string, decimal>();
            }

            return anterior.Itens
                .Where(i => i.Ppl.HasValue)
                .GroupBy(i => i.ChaveCombustivel)
                .ToDictionary(g => g.Key, g => g.First().Ppl!.Value);
        }

        // ------------------------------------------------------------------
        //  Resultado
        // ------------------------------------------------------------------

        public IActionResult Detalhes(int id)
        {
            var usuario = UsuarioLogado();
            if (usuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var inventario = CarregarInventario(id);
            if (inventario == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Administrador = usuario.Administrador;
            ViewBag.Grupos = inventario.GruposDeCombustivel();
            return View(inventario);
        }

        public IActionResult Apagar(int id)
        {
            var usuario = UsuarioLogado();
            if (usuario == null || !usuario.Administrador)
            {
                return RedirectToAction("Login", "Auth");
            }

            var inventario = context.Inventarios.Find(id);
            if (inventario != null)
            {
                inventario.Status = false;
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
