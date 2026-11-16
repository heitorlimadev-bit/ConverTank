using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using ConverTank.Services;

namespace ConverTank.Models
{
    /// <summary>
    /// Inventário trimestral de todos os tanques de todos os postos.
    /// Fluxo: CriarFormulario (gera a planilha de entrada) → preencher as medições →
    /// FazerInventario (absorve a planilha e calcula os volumes) → informar o PPL de cada combustível.
    /// </summary>
    public class Inventario
    {
        // Layout da planilha de entrada
        public const string NomeAba = "Leituras";
        public const int LinhaCabecalho = 5;
        public const string ColunaId = "ID TANQUE";
        public const string ColunaMedicao = "MEDIÇÃO (cm)";
        public const string ColunaData = "DATA DA LEITURA";
        public const string ColunaObservacao = "OBSERVAÇÃO";

        public int Id { get; set; }
        public int Ano { get; set; }
        public int Trimestre { get; set; }
        public DateTime DataReferencia { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataImportacao { get; set; }
        public string? Observacao { get; set; }
        public bool Status { get; set; } = true;

        public List<ItemInventario> Itens { get; set; } = new List<ItemInventario>();

        [NotMapped]
        public string Descricao => $"{Trimestre}º trimestre de {Ano}";

        [NotMapped]
        public string Codigo => $"{Ano}-T{Trimestre}";

        [NotMapped]
        public int TanquesMedidos => Itens.Count(i => i.AlturaCm.HasValue);

        [NotMapped]
        public int TanquesComPreco => Itens.Count(i => i.Ppl.HasValue);

        [NotMapped]
        public double VolumeTotalLitros => Itens.Sum(i => i.VolumeLitros ?? 0);

        [NotMapped]
        public decimal ValorTotal => Itens.Sum(i => i.ValorTotal ?? 0m);

        // =====================================================================
        //  Criação
        // =====================================================================

        public static Inventario Novo(int ano, int trimestre)
        {
            if (trimestre < 1 || trimestre > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(trimestre), "O trimestre deve estar entre 1 e 4.");
            }

            return new Inventario
            {
                Ano = ano,
                Trimestre = trimestre,
                DataReferencia = UltimoDiaDoTrimestre(ano, trimestre),
                DataCriacao = DateTime.Now,
                Status = true
            };
        }

        public static DateTime UltimoDiaDoTrimestre(int ano, int trimestre) =>
            new DateTime(ano, trimestre * 3, 1).AddMonths(1).AddDays(-1);

        /// <summary>Último trimestre já encerrado em relação à data informada.</summary>
        public static (int Ano, int Trimestre) UltimoTrimestreFechado(DateTime hoje)
        {
            int atual = (hoje.Month - 1) / 3 + 1;
            return atual == 1 ? (hoje.Year - 1, 4) : (hoje.Year, atual - 1);
        }

        /// <summary>
        /// Garante que exista um item para cada tanque instalado (PostoTanque ativo, de modelo ativo)
        /// nos postos informados. Itens já existentes mantêm leitura e preço; apenas nome do posto,
        /// numeração e identificação são atualizados.
        /// Os postos devem vir com PostoTanques → Tanque e PostoTanques → Combustivel → Fornecedor.
        /// </summary>
        public void SincronizarTanques(IEnumerable<Posto> postos)
        {
            foreach (var posto in postos.Where(p => p.Status).OrderBy(p => p.Nome))
            {
                int numero = 0;
                var instalados = posto.PostoTanques
                    .Where(pt => pt.Status && pt.Tanque != null && pt.Tanque.Status)
                    .OrderBy(pt => pt.Id);

                foreach (var postoTanque in instalados)
                {
                    numero++;
                    var existente = Itens.FirstOrDefault(i => i.PostoTanqueId == postoTanque.Id);
                    if (existente == null)
                    {
                        Itens.Add(ItemInventario.De(postoTanque, posto, numero));
                    }
                    else if (!existente.Ppl.HasValue)
                    {
                        existente.AtualizarSnapshot(postoTanque, posto, numero);
                    }
                    else
                    {
                        existente.PostoNome = posto.Nome;
                        existente.NumeroNoPosto = numero;
                        existente.Identificacao = string.IsNullOrWhiteSpace(postoTanque.Identificacao) ? null : postoTanque.Identificacao.Trim();
                    }
                }
            }
        }

        // =====================================================================
        //  Formulário de entrada (Excel)
        // =====================================================================

        /// <summary>
        /// Gera a planilha de entrada: uma linha por tanque, agrupada por posto, com a
        /// descrição do tanque (combustível, fornecedor, capacidade em m³) e a coluna
        /// MEDIÇÃO (cm) em amarelo para ser preenchida. Se postos forem informados,
        /// sincroniza os tanques antes de gerar.
        /// </summary>
        public byte[] CriarFormulario(IEnumerable<Posto>? postos = null)
        {
            if (postos != null)
            {
                SincronizarTanques(postos);
            }

            var br = CultureInfo.GetCultureInfo("pt-BR");
            var corCabecalho = XLColor.FromHtml("#1F2937");
            var corEntrada = XLColor.FromHtml("#FFF2CC");
            var corFaixa = XLColor.FromHtml("#F3F4F6");
            var corChave = XLColor.FromHtml("#6B7280");

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(NomeAba);

            // ---- Cabeçalho do documento ----
            ws.Cell(1, 1).Value = "ConverTank — Formulário de Inventário de Tanques";
            ws.Range(1, 1, 1, 10).Merge();
            ws.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(14);

            ws.Cell(2, 1).Value = "Inventário nº";
            ws.Cell(2, 2).Value = Id;
            ws.Cell(2, 3).Value = "Trimestre";
            ws.Cell(2, 4).Value = $"{Trimestre}º/{Ano}";
            ws.Cell(2, 5).Value = "Data de referência";
            ws.Cell(2, 6).Value = DataReferencia;
            ws.Cell(2, 6).Style.NumberFormat.Format = "dd/mm/yyyy";
            ws.Cell(2, 7).Value = "Gerado em";
            ws.Cell(2, 8).Value = DateTime.Now;
            ws.Cell(2, 8).Style.NumberFormat.Format = "dd/mm/yyyy hh:mm";
            foreach (var c in new[] { 1, 3, 5, 7 })
            {
                ws.Cell(2, c).Style.Font.SetBold();
            }
            ws.Cell(2, 2).Style.Font.SetBold().Font.SetFontColor(XLColor.FromHtml("#B45309"));

            ws.Cell(3, 1).Value =
                "Preencha SOMENTE a coluna amarela MEDIÇÃO (cm) com a leitura da régua do último dia do trimestre " +
                $"({DataReferencia.ToString("dd/MM/yyyy", br)}). Aceita vírgula ou ponto decimal. " +
                "Deixe em branco o tanque sem leitura. DATA DA LEITURA é opcional (padrão: data de referência). " +
                "Não altere a coluna ID TANQUE nem o nº do inventário.";
            ws.Range(3, 1, 3, 10).Merge();
            ws.Cell(3, 1).Style.Alignment.SetWrapText(true).Font.SetItalic();
            ws.Row(3).Height = 45;

            // ---- Cabeçalho da tabela ----
            string[] cabecalhos =
            {
                ColunaId, "POSTO", "TANQUE", "COMBUSTÍVEL", "FORNECEDOR", "CAPACIDADE (m³)",
                "DESCRIÇÃO", ColunaMedicao, ColunaData, ColunaObservacao
            };
            for (int c = 0; c < cabecalhos.Length; c++)
            {
                ws.Cell(LinhaCabecalho, c + 1).Value = cabecalhos[c];
            }
            var cab = ws.Range(LinhaCabecalho, 1, LinhaCabecalho, cabecalhos.Length);
            cab.Style.Font.SetBold().Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(corCabecalho);
            cab.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(LinhaCabecalho, 8).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#B45309"));

            // ---- Linhas: uma por tanque ----
            int linha = LinhaCabecalho;
            string? postoAnterior = null;
            bool faixa = false;

            foreach (var item in Itens.OrderBy(i => i.PostoNome).ThenBy(i => i.NumeroNoPosto))
            {
                linha++;
                if (item.PostoNome != postoAnterior)
                {
                    faixa = !faixa;
                    postoAnterior = item.PostoNome;
                }

                ws.Cell(linha, 1).Value = item.PostoTanqueId;
                ws.Cell(linha, 2).Value = item.PostoNome;
                ws.Cell(linha, 3).Value = item.Rotulo;
                ws.Cell(linha, 4).Value = item.Combustivel;
                ws.Cell(linha, 5).Value = item.FornecedorNome;
                ws.Cell(linha, 6).Value = item.CapacidadeM3;
                ws.Cell(linha, 7).Value = $"{item.PostoNome} / {item.Rotulo} – {item.Combustivel} {item.CapacidadeM3.ToString("0.##", br)} m³";
                if (item.AlturaCm.HasValue)
                {
                    ws.Cell(linha, 8).Value = item.AlturaCm.Value;
                }
                if (item.DataLeitura.HasValue && item.DataLeitura.Value.Date != DataReferencia.Date)
                {
                    ws.Cell(linha, 9).Value = item.DataLeitura.Value;
                }
                if (!string.IsNullOrWhiteSpace(item.Observacao))
                {
                    ws.Cell(linha, 10).Value = item.Observacao;
                }

                var linhaRange = ws.Range(linha, 1, linha, 10);
                if (faixa)
                {
                    linhaRange.Style.Fill.SetBackgroundColor(corFaixa);
                }
                ws.Cell(linha, 1).Style.Font.SetFontColor(corChave);
                ws.Cell(linha, 2).Style.Font.SetBold();
                ws.Cell(linha, 8).Style.Fill.SetBackgroundColor(corEntrada).Font.SetBold();
            }

            int ultimaLinha = Math.Max(linha, LinhaCabecalho + 1);
            var tabela = ws.Range(LinhaCabecalho, 1, ultimaLinha, 10);
            tabela.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            tabela.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
            ws.Range(LinhaCabecalho + 1, 6, ultimaLinha, 6).Style.NumberFormat.Format = "0.00";
            ws.Range(LinhaCabecalho + 1, 8, ultimaLinha, 8).Style.NumberFormat.Format = "0.0";
            ws.Range(LinhaCabecalho + 1, 9, ultimaLinha, 9).Style.NumberFormat.Format = "dd/mm/yyyy";
            ws.Range(LinhaCabecalho + 1, 1, ultimaLinha, 6).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Range(LinhaCabecalho + 1, 2, ultimaLinha, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            ws.Range(LinhaCabecalho, 1, ultimaLinha, 10).SetAutoFilter();

            ws.Column(1).Width = 11;
            ws.Column(2).Width = 22;
            ws.Column(3).Width = 9;
            ws.Column(4).Width = 14;
            ws.Column(5).Width = 20;
            ws.Column(6).Width = 16;
            ws.Column(7).Width = 42;
            ws.Column(8).Width = 16;
            ws.Column(9).Width = 17;
            ws.Column(10).Width = 30;
            ws.SheetView.FreezeRows(LinhaCabecalho);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // =====================================================================
        //  Planilha de saída (estoque valorizado por posto)
        // =====================================================================

        // Nome de exibição e ordem dos combustíveis na planilha de saída
        private static readonly (string Sigla, string Nome)[] NomesCombustivel =
        {
            ("GC", "G. COMUM"),
            ("GA", "G. ADITIVADA"),
            ("GP", "G. PODIUM"),
            ("E", "ETANOL"),
            ("D", "DIESEL S500"),
            ("S500", "DIESEL S500"),
            ("S10", "DIESEL S10"),
            ("ARLA", "ARLA 32"),
        };

        public static string NomeCombustivelSaida(string combustivel)
        {
            var sigla = (combustivel ?? string.Empty).Trim().ToUpperInvariant();
            foreach (var (s, nome) in NomesCombustivel)
            {
                if (s == sigla) return nome;
            }
            return sigla;
        }

        private static int OrdemCombustivel(string combustivel)
        {
            var nome = NomeCombustivelSaida(combustivel);
            int i = Array.FindIndex(NomesCombustivel, n => n.Nome == nome);
            return i < 0 ? 99 : i;
        }

        /// <summary>
        /// Gera a planilha de saída no formato "ESTOQUE dd/mm/aaaa": um bloco por posto
        /// (POSTO | QTDADE | PREÇO | TOTAL), em duas colunas, com uma linha por combustível
        /// (tanques do mesmo combustível e mesmo preço no posto são somados), subtotal
        /// do posto e total geral. Os totais ficam como fórmulas do Excel.
        /// Pode ser gerada a qualquer momento para inventários já importados; tanques
        /// sem leitura entram com quantidade 0 e sem PPL ficam com preço em branco.
        /// </summary>
        public byte[] CriarPlanilhaSaida(string nomeGrupo = "GRUPO ITAMARATI")
        {
            var postos = Itens
                .GroupBy(i => i.PostoNome)
                .OrderBy(g => g.Key, StringComparer.Create(CultureInfo.GetCultureInfo("pt-BR"), true))
                .Select(g => new
                {
                    Nome = g.Key.ToUpper(CultureInfo.GetCultureInfo("pt-BR")),
                    Linhas = g.GroupBy(i => new { Nome = NomeCombustivelSaida(i.Combustivel), i.Ppl })
                              .Select(l => new
                              {
                                  l.Key.Nome,
                                  l.Key.Ppl,
                                  Ordem = OrdemCombustivel(l.First().Combustivel),
                                  Litros = Math.Round(l.Sum(i => i.VolumeLitros ?? 0), 0)
                              })
                              .OrderBy(l => l.Ordem).ThenBy(l => l.Nome).ThenBy(l => l.Ppl)
                              .ToList()
                })
                .ToList();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Estoque");
            ws.Style.Font.FontName = "Calibri";
            ws.Style.Font.FontSize = 11;

            ws.Column(1).Width = 0.43;
            ws.Column(2).Width = 34;   // B  posto / combustível
            ws.Column(3).Width = 10.6; // C  quantidade
            ws.Column(4).Width = 10;   // D  preço
            ws.Column(5).Width = 15.6; // E  total
            ws.Column(6).Width = 6.3;  // F  espaço
            ws.Column(7).Width = 34;   // G  posto / combustível
            ws.Column(8).Width = 10.6; // H  quantidade
            ws.Column(9).Width = 10;   // I  preço
            ws.Column(10).Width = 15.6; // J total

            ws.Cell(3, 2).Value = $"ESTOQUE  {DataReferencia:dd/MM/yyyy}";
            ws.Range(3, 2, 3, 10).Merge();
            ws.Cell(3, 2).Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Row(3).Height = 21;

            ws.Cell(4, 5).Value = nomeGrupo;
            ws.Range(4, 5, 4, 7).Merge();
            ws.Cell(4, 5).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            const int linhaInicial = 7;
            int[] cursor = { linhaInicial, linhaInicial }; // próxima linha livre em cada coluna
            int[] colunaInicial = { 2, 7 };                 // B e G
            var subtotais = new List<string>();

            foreach (var posto in postos)
            {
                // Coloca o bloco na coluna que estiver mais curta
                int lado = cursor[0] <= cursor[1] ? 0 : 1;
                int c0 = colunaInicial[lado];
                int r = cursor[lado];
                string colQtd = XLHelper.GetColumnLetterFromNumber(c0 + 1);
                string colPreco = XLHelper.GetColumnLetterFromNumber(c0 + 2);
                string colTotal = XLHelper.GetColumnLetterFromNumber(c0 + 3);

                // Cabeçalho do posto
                ws.Cell(r, c0).Value = posto.Nome;
                ws.Cell(r, c0 + 1).Value = "QTDADE";
                ws.Cell(r, c0 + 2).Value = "PREÇO";
                ws.Cell(r, c0 + 3).Value = "TOTAL";
                var cab = ws.Range(r, c0, r, c0 + 3);
                cab.Style.Font.SetBold();
                ws.Range(r, c0 + 1, r, c0 + 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                int primeira = r + 1;
                foreach (var linha in posto.Linhas)
                {
                    r++;
                    ws.Cell(r, c0).Value = linha.Nome;
                    ws.Cell(r, c0 + 1).Value = linha.Litros;
                    if (linha.Ppl.HasValue)
                    {
                        ws.Cell(r, c0 + 2).Value = (double)linha.Ppl.Value;
                    }
                    ws.Cell(r, c0 + 3).FormulaA1 = $"{colQtd}{r}*{colPreco}{r}";
                }
                int ultima = r;

                // Subtotal do posto
                r++;
                ws.Cell(r, c0 + 3).FormulaA1 = $"SUM({colTotal}{primeira}:{colTotal}{ultima})";
                ws.Cell(r, c0 + 3).Style.Font.SetBold();
                subtotais.Add($"{colTotal}{r}");

                var bloco = ws.Range(primeira - 1, c0, r - 1, c0 + 3);
                bloco.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                bloco.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
                ws.Cell(r, c0 + 3).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                ws.Range(primeira, c0 + 1, ultima, c0 + 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Range(primeira, c0 + 1, ultima, c0 + 1).Style.NumberFormat.Format = "#,##0";
                ws.Range(primeira, c0 + 2, ultima, c0 + 2).Style.NumberFormat.Format = "0.0000";
                ws.Range(primeira, c0 + 3, r, c0 + 3).Style.NumberFormat.Format = "#,##0.00";

                cursor[lado] = r + 1;
            }

            // Total geral
            if (subtotais.Count > 0)
            {
                int linhaTotal = Math.Max(cursor[0], cursor[1]) + 1;
                ws.Cell(linhaTotal, 7).Value = "TOTAL GERAL";
                ws.Range(linhaTotal, 7, linhaTotal, 9).Merge();
                ws.Cell(linhaTotal, 10).FormulaA1 = string.Join("+", subtotais);
                var total = ws.Range(linhaTotal, 7, linhaTotal, 10);
                total.Style.Font.SetBold().Font.SetFontSize(12);
                total.Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
                ws.Cell(linhaTotal, 10).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(linhaTotal, 10).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }

            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            ws.PageSetup.FitToPages(1, 0);
            ws.PageSetup.PrintAreas.Add(1, 1, Math.Max(cursor[0], cursor[1]) + 1, 10);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // =====================================================================
        //  Importação da planilha preenchida
        // =====================================================================

        /// <summary>
        /// Absorve a planilha preenchida: para cada linha com ID TANQUE e MEDIÇÃO (cm),
        /// calcula o volume em litros via TanqueService e registra a medição no histórico do tanque.
        /// Os itens precisam estar carregados com PostoTanque → Tanque (e Medicao, se já existir).
        /// Linhas em branco não apagam leituras importadas anteriormente.
        /// </summary>
        public ResultadoImportacao FazerInventario(Stream planilha)
        {
            var resultado = new ResultadoImportacao();

            using var copia = new MemoryStream();
            planilha.CopyTo(copia);
            copia.Position = 0;

            XLWorkbook wb;
            try
            {
                wb = new XLWorkbook(copia);
            }
            catch (Exception ex)
            {
                resultado.Erros.Add($"Não foi possível abrir a planilha: {ex.Message}");
                return resultado;
            }

            using (wb)
            {
                var ws = wb.Worksheets.FirstOrDefault(w => w.Name.Equals(NomeAba, StringComparison.OrdinalIgnoreCase))
                         ?? wb.Worksheets.First();

                // Confere se a planilha é deste inventário
                var numeroPlanilha = LerNumero(ws.Cell(2, 2));
                if (Id > 0 && numeroPlanilha.HasValue && (int)numeroPlanilha.Value != Id)
                {
                    resultado.Erros.Add($"Esta planilha pertence ao inventário nº {(int)numeroPlanilha.Value}, e não ao nº {Id}.");
                    return resultado;
                }

                // Localiza a linha de cabeçalho e as colunas pelo nome (tolerante a reordenação)
                int linhaCab = 0;
                var colunas = new Dictionary<string, int>();
                int ultimaLinhaUsada = ws.LastRowUsed()?.RowNumber() ?? 0;
                int ultimaColunaUsada = ws.LastColumnUsed()?.ColumnNumber() ?? 0;

                for (int r = 1; r <= Math.Min(30, ultimaLinhaUsada) && linhaCab == 0; r++)
                {
                    for (int c = 1; c <= ultimaColunaUsada; c++)
                    {
                        if (Normalizar(ws.Cell(r, c).GetString()) == Normalizar(ColunaId))
                        {
                            linhaCab = r;
                            break;
                        }
                    }
                }

                if (linhaCab == 0)
                {
                    resultado.Erros.Add($"Cabeçalho \"{ColunaId}\" não encontrado. Use a planilha gerada pelo sistema.");
                    return resultado;
                }

                for (int c = 1; c <= ultimaColunaUsada; c++)
                {
                    var nome = Normalizar(ws.Cell(linhaCab, c).GetString());
                    if (nome.Length > 0 && !colunas.ContainsKey(nome))
                    {
                        colunas[nome] = c;
                    }
                }

                if (!colunas.TryGetValue(Normalizar(ColunaMedicao), out int colMedicao))
                {
                    resultado.Erros.Add($"Coluna \"{ColunaMedicao}\" não encontrada.");
                    return resultado;
                }
                int colId = colunas[Normalizar(ColunaId)];
                colunas.TryGetValue(Normalizar(ColunaData), out int colData);
                colunas.TryGetValue(Normalizar(ColunaObservacao), out int colObs);

                var vistos = new HashSet<int>();

                for (int r = linhaCab + 1; r <= ultimaLinhaUsada; r++)
                {
                    var idCelula = LerNumero(ws.Cell(r, colId));
                    if (!idCelula.HasValue)
                    {
                        continue; // linha vazia ou de separação
                    }

                    int tanqueId = (int)idCelula.Value;
                    var item = Itens.FirstOrDefault(i => i.PostoTanqueId == tanqueId);
                    if (item == null)
                    {
                        resultado.Avisos.Add($"Linha {r}: tanque ID {tanqueId} não faz parte deste inventário — ignorado.");
                        continue;
                    }
                    if (!vistos.Add(tanqueId))
                    {
                        resultado.Avisos.Add($"Linha {r}: tanque ID {tanqueId} repetido — usada a última ocorrência.");
                    }

                    if (colObs > 0)
                    {
                        var obs = ws.Cell(r, colObs).GetString().Trim();
                        if (obs.Length > 0)
                        {
                            item.Observacao = obs;
                        }
                    }

                    var celulaMedicao = ws.Cell(r, colMedicao);
                    if (celulaMedicao.Value.IsBlank || celulaMedicao.GetString().Trim().Length == 0)
                    {
                        continue;
                    }

                    var altura = LerNumero(celulaMedicao);
                    if (!altura.HasValue)
                    {
                        resultado.Erros.Add($"Linha {r} ({item.PostoNome} {item.Rotulo}): medição \"{celulaMedicao.GetString()}\" não é um número.");
                        continue;
                    }
                    if (altura.Value < 0)
                    {
                        resultado.Erros.Add($"Linha {r} ({item.PostoNome} {item.Rotulo}): medição negativa ({altura.Value}).");
                        continue;
                    }

                    var tanque = item.PostoTanque?.Tanque;
                    if (tanque == null)
                    {
                        resultado.Erros.Add($"Tanque ID {tanqueId} não carregado. (Inclua Itens.PostoTanque.Tanque na consulta.)");
                        continue;
                    }

                    double diametroCm = tanque.Raio * 200.0;
                    if (diametroCm > 0 && altura.Value > diametroCm + 0.5)
                    {
                        resultado.Avisos.Add(
                            $"{item.PostoNome} {item.Rotulo}: medição {altura.Value:0.#} cm maior que o diâmetro ({diametroCm:0.#} cm) — considerado cheio. Confira a leitura.");
                    }

                    DateTime dataLeitura = DataReferencia;
                    if (colData > 0)
                    {
                        dataLeitura = LerData(ws.Cell(r, colData)) ?? DataReferencia;
                    }

                    item.AlturaCm = altura.Value;
                    item.VolumeLitros = TanqueService.CalcularVolume(tanque.Raio, tanque.Comprimento, altura.Value);
                    item.DataLeitura = dataLeitura;
                    item.RegistrarMedicao();
                    resultado.Preenchidos++;
                }

                foreach (var semLeitura in Itens.Where(i => !i.AlturaCm.HasValue)
                                                 .OrderBy(i => i.PostoNome).ThenBy(i => i.NumeroNoPosto))
                {
                    resultado.SemLeitura.Add($"{semLeitura.PostoNome} {semLeitura.Rotulo} ({semLeitura.Combustivel})");
                }
            }

            if (resultado.Preenchidos > 0)
            {
                DataImportacao = DateTime.Now;
            }
            return resultado;
        }

        // =====================================================================
        //  Preço por litro (PPL)
        // =====================================================================

        /// <summary>
        /// Altera o PPL de todos os tanques do inventário cujo combustível tem o mesmo nome
        /// e o mesmo fornecedor. Retorna quantos tanques foram alterados.
        /// </summary>
        public int AlterarPpl(string combustivel, int fornecedorId, decimal? ppl)
        {
            if (ppl.HasValue && ppl.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ppl), "O preço por litro não pode ser negativo.");
            }

            var chave = ItemInventario.Chave(combustivel, fornecedorId);
            int alterados = 0;
            foreach (var item in Itens.Where(i => i.ChaveCombustivel == chave))
            {
                item.Ppl = ppl;
                alterados++;
            }
            return alterados;
        }

        /// <summary>Agrupa os tanques por combustível (nome + fornecedor).</summary>
        public List<GrupoCombustivel> GruposDeCombustivel() =>
            Itens.GroupBy(i => i.ChaveCombustivel)
                 .Select(g => new GrupoCombustivel
                 {
                     Chave = g.Key,
                     Combustivel = g.First().Combustivel,
                     FornecedorId = g.First().FornecedorId,
                     FornecedorNome = g.First().FornecedorNome,
                     Itens = g.OrderBy(i => i.PostoNome).ThenBy(i => i.NumeroNoPosto).ToList()
                 })
                 .OrderBy(g => g.Combustivel).ThenBy(g => g.FornecedorNome)
                 .ToList();

        // =====================================================================
        //  Utilitários de leitura
        // =====================================================================

        private static string Normalizar(string texto)
        {
            var semAcento = new StringBuilder();
            foreach (var ch in (texto ?? string.Empty).Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                {
                    semAcento.Append(ch);
                }
            }
            return semAcento.ToString();
        }

        private static double? LerNumero(IXLCell celula)
        {
            var valor = celula.Value;
            if (valor.IsBlank)
            {
                return null;
            }
            if (valor.IsNumber)
            {
                return valor.GetNumber();
            }
            if (valor.IsText && TentarLerDecimal(valor.GetText(), out var d))
            {
                return (double)d;
            }
            return null;
        }

        private static DateTime? LerData(IXLCell celula)
        {
            var valor = celula.Value;
            if (valor.IsDateTime)
            {
                return valor.GetDateTime();
            }
            if (valor.IsNumber)
            {
                try { return DateTime.FromOADate(valor.GetNumber()); } catch { return null; }
            }
            if (valor.IsText)
            {
                var formatos = new[] { "dd/MM/yyyy", "d/M/yyyy", "dd/MM/yy", "yyyy-MM-dd" };
                if (DateTime.TryParseExact(valor.GetText().Trim(), formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                {
                    return data;
                }
            }
            return null;
        }

        /// <summary>Lê números digitados no padrão brasileiro ou americano: "5,899", "5.899", "R$ 5,89", "123,5 cm", "1.234,56".</summary>
        public static bool TentarLerDecimal(string? texto, out decimal valor)
        {
            valor = 0;
            if (string.IsNullOrWhiteSpace(texto))
            {
                return false;
            }

            var limpo = new string(texto.Where(ch => char.IsDigit(ch) || ch == ',' || ch == '.' || ch == '-').ToArray());
            if (limpo.Length == 0)
            {
                return false;
            }

            int ultimaVirgula = limpo.LastIndexOf(',');
            int ultimoPonto = limpo.LastIndexOf('.');
            if (ultimaVirgula >= 0 && ultimoPonto >= 0)
            {
                // O separador que aparece por último é o decimal
                limpo = ultimaVirgula > ultimoPonto
                    ? limpo.Replace(".", "").Replace(',', '.')
                    : limpo.Replace(",", "");
            }
            else
            {
                limpo = limpo.Replace(',', '.');
            }

            return decimal.TryParse(limpo, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
        }
    }

    public class ResultadoImportacao
    {
        public int Preenchidos { get; set; }
        public List<string> SemLeitura { get; set; } = new List<string>();
        public List<string> Avisos { get; set; } = new List<string>();
        public List<string> Erros { get; set; } = new List<string>();
        public bool Sucesso => Erros.Count == 0;
    }

    public class GrupoCombustivel
    {
        public string Chave { get; set; } = string.Empty;
        public string Combustivel { get; set; } = string.Empty;
        public int FornecedorId { get; set; }
        public string FornecedorNome { get; set; } = string.Empty;
        public List<ItemInventario> Itens { get; set; } = new List<ItemInventario>();

        public double VolumeLitros => Itens.Sum(i => i.VolumeLitros ?? 0);
        public decimal ValorTotal => Itens.Sum(i => i.ValorTotal ?? 0m);

        /// <summary>PPL comum a todos os tanques do grupo (null se vazio ou divergente).</summary>
        public decimal? PplComum
        {
            get
            {
                var precos = Itens.Select(i => i.Ppl).Distinct().ToList();
                return precos.Count == 1 ? precos[0] : null;
            }
        }
    }
}
