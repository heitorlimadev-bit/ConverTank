namespace ConverTank.Models
{
    public class Combustivel
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public int FornecedorId { get; set; }

        public Fornecedor Fornecedor { get; set; }


    }
}
