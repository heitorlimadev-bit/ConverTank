namespace ConverTank.Models
{
    public class Posto : Entidade
    {

        public List <Tanque> Tanques { get; set;} = new List<Tanque>();

        public List <UsuarioPosto> UsuarioPostos { get; set;}
        
    }

}
