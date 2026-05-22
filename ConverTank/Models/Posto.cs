namespace ConverTank.Models
{
    public class Posto
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public string Cnpj { get; set; }

        public List <Tanque> Tanques { get; set;} = new List<Tanque>();
        
    }

}
