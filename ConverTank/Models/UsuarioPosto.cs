namespace ConverTank.Models
{
    public class UsuarioPosto
    {

        public Usuario Usuario {  get; set; }
        public int UsuarioId { get; set; }
        public Posto Posto { get; set; }
        public int PostoId { get; set; }

        public List<UsuarioPosto> UsuarioPostos { get; set; }

    }
}
