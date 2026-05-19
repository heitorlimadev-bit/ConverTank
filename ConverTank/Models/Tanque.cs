namespace ConverTank.Models
{
    public class Tanque
    {

        public int Id { get; set; }
        public string Fabricante { get; set; }
        public int Raio { get; set; }
        public int Comprimento { get; set; }
        public int PostoId { get ; set; }
        public Posto Posto { get; set; }
    }
}
