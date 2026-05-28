namespace ConverTank.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Login { get; set; }
        public string Senha { get; set; }

        public bool Administrador { get; set; } = false;
        public List<UsuarioPosto> UsuarioPostos { get; set; }

    }
}
