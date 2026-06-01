namespace ConverTank.Models
{
    public class Medicao
    {

        public int Id { get; set; }

        public double Altura { get; set; }

        public double Volume { get; set; }

        public int TanqueId { get; set; }

        public Tanque Tanque { get; set; }

        public DateTime DataMedicao { get; set; }

        public bool Status { get; set; } = true;

    }
}
