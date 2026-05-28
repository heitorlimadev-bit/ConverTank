using ConverTank.Models;
using Microsoft.EntityFrameworkCore;

namespace ConverTank.Data
{
    public class ConteudoBanco : DbContext
    {
        
        public ConteudoBanco   (DbContextOptions <ConteudoBanco> Options ) : base ( Options ) { }

        public DbSet<Entidade> Entidades { get; set; }

        public DbSet<Posto> Postos { get; set; }

        public DbSet<Tanque> Tanques { get; set; }

        public DbSet<Medicao> Medicoes { get; set; }

        public DbSet<Combustivel> Combustiveis { get; set; }

        public DbSet<Fornecedor> Fornecedores { get; set; }

        public DbSet<Fabricante> Fabricantes { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<UsuarioPosto> UsuariosPostos { get;set; }



    }
}
