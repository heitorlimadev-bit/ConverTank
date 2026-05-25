namespace ConverTank.Models
{
    public class Tanque
    {
        public int Id { get; set; }

        public Fabricante Fabricante { get; set; }

        public int FabricanteId { get; set; }

        public int Volume { get; set; }

        public double Raio { get; set; }

        public double Comprimento { get; set; }

        public int PostoId { get; set; }

        public Posto Posto { get; set; }

        public Combustivel Combustivel { get; set; }

        public int CombustivelId { get; set; }

        public List<Medicao> Medicoes { get; set; } = new List<Medicao>();



    }
}
