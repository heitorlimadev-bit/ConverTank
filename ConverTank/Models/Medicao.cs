namespace ConverTank.Models
{
    public class Medicao
    {

        public int Id { get; set; }

        public int Altura { get; set; }

        public double Volume { get; set; }

        public int TanqueId { get; set; }

        public Tanque Tanque { get; set; }

        public DateTime DataMedicao { get; set; }

    }
}
