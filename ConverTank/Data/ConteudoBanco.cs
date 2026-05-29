using ConverTank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsuarioPosto>().HasKey(up => new
            {

                up.UsuarioId,
                up.PostoId

            });
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tanque>().HasOne(t => t.Posto).WithMany(p => p.Tanques).HasForeignKey(t => t.PostoId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Tanque>().HasOne(t => t.Fabricante).WithMany(p => p.Tanques).HasForeignKey(t => t.FabricanteId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Entidade>().ToTable("Entidades");
            modelBuilder.Entity<Posto>().ToTable("Postos");
            modelBuilder.Entity<Fornecedor>().ToTable("Fornecedores");
            modelBuilder.Entity<Fabricante>().ToTable("Fabricantes");

        }

    }
}
