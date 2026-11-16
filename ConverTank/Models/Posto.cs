namespace ConverTank.Models
{
    public class Posto : Entidade
    {

        /// <summary>Tanques instalados neste posto (relação N:N com o modelo de tanque).</summary>
        public List<PostoTanque> PostoTanques { get; set; } = new List<PostoTanque>();

        public List <UsuarioPosto> UsuarioPostos { get; set;}
        
    }

}
