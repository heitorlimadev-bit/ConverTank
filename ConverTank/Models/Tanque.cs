using System.ComponentModel.DataAnnotations.Schema;

namespace ConverTank.Models
{
    /// <summary>
    /// MODELO de tanque (especificação física): dimensões, capacidade e fabricante.
    /// Um mesmo modelo pode estar instalado em vários postos e várias vezes no mesmo posto —
    /// cada instalação é um <see cref="PostoTanque"/>.
    /// </summary>
    public class Tanque
    {
        public int Id { get; set; }

        /// <summary>Nome livre para identificar o modelo (ex.: "15 m³ Zeppini bipartido").</summary>
        public string? Nome { get; set; }

        /// <summary>Volume nominal em litros.</summary>
        public int Volume { get; set; }
        public double Raio { get; set; }
        public double Comprimento { get; set; }

        [NotMapped]
        public double Diametro
        {
            get => Math.Round(Raio * 2.0, 4);
            set => Raio = value / 2.0;
        }

        public Fabricante Fabricante { get; set; }
        [ForeignKey(nameof(Fabricante))]
        public int FabricanteId { get; set; }

        public bool Status { get; set; } = true;

        /// <summary>Instalações deste modelo nos postos.</summary>
        public List<PostoTanque> PostoTanques { get; set; } = new List<PostoTanque>();

        [NotMapped]
        public string Descricao
        {
            get
            {
                var medidas = $"{Volume / 1000.0:0.##} m³ · Ø {Diametro:0.00} × {Comprimento:0.00} m";
                return string.IsNullOrWhiteSpace(Nome) ? medidas : $"{Nome} ({medidas})";
            }
        }
    }
}
