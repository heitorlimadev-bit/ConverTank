using System.ComponentModel.DataAnnotations.Schema;

namespace ConverTank.Models
{
    /// <summary>
    /// Relação N:N entre Posto e Tanque (modelo): um tanque físico instalado num posto.
    /// Tem chave própria (Id), por isso o mesmo modelo pode aparecer várias vezes no mesmo posto.
    /// O combustível fica aqui, pois o mesmo modelo guarda combustíveis diferentes em cada instalação.
    /// As medições pertencem à instalação, não ao modelo.
    /// </summary>
    public class PostoTanque
    {
        public int Id { get; set; }

        public int PostoId { get; set; }
        public Posto Posto { get; set; }

        public int TanqueId { get; set; }
        public Tanque Tanque { get; set; }

        public int CombustivelId { get; set; }
        public Combustivel Combustivel { get; set; }

        /// <summary>Identificação opcional no posto (ex.: "T1", "Bipartido A"). Vazio = numeração automática.</summary>
        public string? Identificacao { get; set; }

        public bool Status { get; set; } = true;

        public List<Medicao> Medicoes { get; set; } = new List<Medicao>();

        [NotMapped]
        public string Rotulo => string.IsNullOrWhiteSpace(Identificacao) ? $"#{Id}" : Identificacao!;
    }
}
