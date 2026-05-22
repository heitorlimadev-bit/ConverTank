using ConverTank.Models;
using Microsoft.EntityFrameworkCore;

namespace ConverTank.Data
{
    public class ConteudoBanco : DbContext
    {
        
        public ConteudoBanco   (DbContextOptions <ConteudoBanco> Options ) : base ( Options )
            
            { }

        public DbSet<Posto> Postos { get; set; }

        public DbSet<Tanque> Tanques { get; set; }

        public DbSet<Medicao> Medicoes { get; set; }



    }
}
