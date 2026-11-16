using System.ComponentModel.DataAnnotations.Schema;

namespace ConverTank.Models
{
    /// <summary>
    /// Linha do inventário: a situação de UM tanque na data de referência do inventário.
    /// Guarda uma "fotografia" (snapshot) dos dados do tanque no momento do inventário,
    /// para que alterações futuras no cadastro não mudem inventários já fechados.
    /// </summary>
    public class ItemInventario
    {
        public int Id { get; set; }

        public int InventarioId { get; set; }
        public Inventario Inventario { get; set; } = null!;

        /// <summary>Tanque instalado no posto (relação N:N Posto × modelo de Tanque).</summary>
        public int PostoTanqueId { get; set; }
        public PostoTanque PostoTanque { get; set; } = null!;

        // Medição gerada no histórico do tanque quando a planilha é importada
        public int? MedicaoId { get; set; }
        public Medicao? Medicao { get; set; }

        // ---- Snapshot do cadastro ----
        public string PostoNome { get; set; } = string.Empty;
        public int NumeroNoPosto { get; set; }
        public string? Identificacao { get; set; }
        public string Combustivel { get; set; } = string.Empty;
        public int FornecedorId { get; set; }
        public string FornecedorNome { get; set; } = string.Empty;
        public double CapacidadeM3 { get; set; }

        // ---- Leitura (vinda da planilha) ----
        public double? AlturaCm { get; set; }
        public double? VolumeLitros { get; set; }
        public DateTime? DataLeitura { get; set; }
        public string? Observacao { get; set; }

        // ---- Preço por litro (R$) ----
        public decimal? Ppl { get; set; }

        [NotMapped]
        public string Rotulo => string.IsNullOrWhiteSpace(Identificacao) ? $"T{NumeroNoPosto}" : Identificacao!;

        [NotMapped]
        public string Descricao => $"{Combustivel} · {FornecedorNome} · {CapacidadeM3:0.##} m³";

        [NotMapped]
        public string ChaveCombustivel => Chave(Combustivel, FornecedorId);

        [NotMapped]
        public decimal? ValorTotal =>
            VolumeLitros.HasValue && Ppl.HasValue
                ? Math.Round((decimal)VolumeLitros.Value * Ppl.Value, 2)
                : null;

        [NotMapped]
        public double? OcupacaoPercentual =>
            VolumeLitros.HasValue && CapacidadeM3 > 0
                ? Math.Round(VolumeLitros.Value / (CapacidadeM3 * 1000.0) * 100.0, 1)
                : null;

        /// <summary>Combustíveis com mesmo nome e mesmo fornecedor compartilham a mesma chave.</summary>
        public static string Chave(string combustivel, int fornecedorId) =>
            $"{(combustivel ?? string.Empty).Trim().ToUpperInvariant()}|{fornecedorId}";

        /// <summary>Capacidade nominal em m³. O cadastro guarda o volume em litros;
        /// se estiver zerado, usa a capacidade geométrica (π·r²·comprimento).</summary>
        public static double CalcularCapacidadeM3(Tanque tanque)
        {
            if (tanque.Volume > 0)
            {
                return Math.Round(tanque.Volume / 1000.0, 2);
            }
            return Math.Round(Math.PI * Math.Pow(tanque.Raio, 2) * tanque.Comprimento, 2);
        }

        public static ItemInventario De(PostoTanque postoTanque, Posto posto, int numeroNoPosto)
        {
            var item = new ItemInventario { PostoTanqueId = postoTanque.Id, PostoTanque = postoTanque };
            item.AtualizarSnapshot(postoTanque, posto, numeroNoPosto);
            return item;
        }

        /// <summary>Requer PostoTanque com Tanque e Combustivel.Fornecedor carregados.</summary>
        public void AtualizarSnapshot(PostoTanque postoTanque, Posto posto, int numeroNoPosto)
        {
            PostoNome = posto.Nome;
            NumeroNoPosto = numeroNoPosto;
            Identificacao = string.IsNullOrWhiteSpace(postoTanque.Identificacao) ? null : postoTanque.Identificacao.Trim();
            Combustivel = postoTanque.Combustivel?.Nome ?? "?";
            FornecedorId = postoTanque.Combustivel?.FornecedorId ?? 0;
            FornecedorNome = postoTanque.Combustivel?.Fornecedor?.Nome ?? "-";
            CapacidadeM3 = postoTanque.Tanque != null ? CalcularCapacidadeM3(postoTanque.Tanque) : 0;
        }

        /// <summary>Cria ou atualiza a medição correspondente no histórico do tanque.</summary>
        public void RegistrarMedicao()
        {
            if (!AlturaCm.HasValue || !VolumeLitros.HasValue)
            {
                return;
            }

            Medicao ??= new Medicao { PostoTanqueId = PostoTanqueId, Status = true };
            Medicao.Altura = AlturaCm.Value;
            Medicao.Volume = VolumeLitros.Value;
            Medicao.DataMedicao = DataLeitura ?? DateTime.Now;
            Medicao.Status = true;
        }
    }
}
