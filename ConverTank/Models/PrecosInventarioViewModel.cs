namespace ConverTank.Models
{
    /// <summary>Preço digitado para um grupo (mesmo combustível + mesmo fornecedor).</summary>
    public class PrecoGrupoInput
    {
        public string Combustivel { get; set; } = string.Empty;
        public int FornecedorId { get; set; }
        public string? Ppl { get; set; }
        public string? PplOriginal { get; set; }
    }

    /// <summary>Preço digitado para um tanque específico (exceção ao preço do grupo).</summary>
    public class PrecoItemInput
    {
        public int Id { get; set; }
        public string? Ppl { get; set; }
        public string? PplOriginal { get; set; }
    }
}
