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

        public DbSet<PostoTanque> PostosTanques { get; set; }

        public DbSet<Medicao> Medicoes { get; set; }

        public DbSet<Combustivel> Combustiveis { get; set; }

        public DbSet<Fornecedor> Fornecedores { get; set; }

        public DbSet<Fabricante> Fabricantes { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<UsuarioPosto> UsuariosPostos { get;set; }

        public DbSet<Inventario> Inventarios { get; set; }

        public DbSet<ItemInventario> ItensInventario { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsuarioPosto>().HasKey(up => new
            {

                up.UsuarioId,
                up.PostoId

            });
            base.OnModelCreating(modelBuilder);

            // N:N Posto × Tanque (modelo) via PostoTanque, com chave própria (permite o mesmo modelo repetido no posto)
            modelBuilder.Entity<PostoTanque>().ToTable("PostosTanques");
            modelBuilder.Entity<PostoTanque>().HasOne(pt => pt.Posto).WithMany(p => p.PostoTanques).HasForeignKey(pt => pt.PostoId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<PostoTanque>().HasOne(pt => pt.Tanque).WithMany(t => t.PostoTanques).HasForeignKey(pt => pt.TanqueId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<PostoTanque>().HasOne(pt => pt.Combustivel).WithMany().HasForeignKey(pt => pt.CombustivelId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Medicao>().HasOne(m => m.PostoTanque).WithMany(pt => pt.Medicoes).HasForeignKey(m => m.PostoTanqueId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tanque>().HasOne(t => t.Fabricante).WithMany(p => p.Tanques).HasForeignKey(t => t.FabricanteId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Entidade>().ToTable("Entidades");
            modelBuilder.Entity<Posto>().ToTable("Postos");
            modelBuilder.Entity<Fornecedor>().ToTable("Fornecedores");
            modelBuilder.Entity<Fabricante>().ToTable("Fabricantes");

            modelBuilder.Entity<Inventario>().HasIndex(i => new { i.Ano, i.Trimestre });
            modelBuilder.Entity<ItemInventario>().HasOne(i => i.Inventario).WithMany(inv => inv.Itens).HasForeignKey(i => i.InventarioId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ItemInventario>().HasOne(i => i.PostoTanque).WithMany().HasForeignKey(i => i.PostoTanqueId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<ItemInventario>().HasOne(i => i.Medicao).WithMany().HasForeignKey(i => i.MedicaoId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<ItemInventario>().Property(i => i.Ppl).HasPrecision(18, 4);

        }

    }
}
