using System.ComponentModel.DataAnnotations.Schema;

namespace ConverTank.Models
{
    public class Tanque
    {
        public int Id { get; set; }
        public int Volume { get; set; }
        public double Raio { get; set; }
        public double Comprimento { get; set; }

        [NotMapped]
        public double Diametro
        {
            get => Math.Round(Raio * 2.0, 4);
            set => Raio = value / 2.0;
        }

        public Posto Posto { get; set; }
        [ForeignKey(nameof(Posto))]
        public int PostoId { get; set; }

        public Combustivel Combustivel { get; set; }
        [ForeignKey(nameof(Combustivel))]
        public int CombustivelId { get; set; }

        public Fabricante Fabricante { get; set; }
        [ForeignKey(nameof(Fabricante))]
        public int FabricanteId { get; set; }

        public bool Status { get; set; } = true;

        public List<Medicao> Medicoes { get; set; } = new List<Medicao>();
    }
}
