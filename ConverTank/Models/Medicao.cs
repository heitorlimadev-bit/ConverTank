namespace ConverTank.Models
{
    public class Medicao
    {
        public int Id { get; set; }
        public double Altura { get; set; }
        public double Volume { get; set; }

        /// <summary>Tanque instalado no posto que foi medido.</summary>
        public int PostoTanqueId { get; set; }
        public PostoTanque PostoTanque { get; set; }

        public DateTime DataMedicao { get; set; }
        public bool Status { get; set; } = true;
    }
}
