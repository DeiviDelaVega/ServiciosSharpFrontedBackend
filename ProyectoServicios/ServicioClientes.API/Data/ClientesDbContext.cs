using Microsoft.EntityFrameworkCore;
using ServicioClientes.API.Models;

namespace ServicioClientes.API.Data
{
    public class ClientesDbContext : DbContext
    {
        public ClientesDbContext(DbContextOptions<ClientesDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<Administrador> Administrador { get; set; }
        public DbSet<Cliente> ClientesMasReservas { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>()
                .HasKey(u => u.Correo);

            modelBuilder.Entity<Administrador>()
                .HasKey(a => a.ID_Administrador);  

            modelBuilder.Entity<Administrador>()
                .HasOne(a => a.Usuario)
                .WithOne(u => u.Administrador)
                .HasForeignKey<Administrador>(a => a.Correo);

            modelBuilder.Entity<Cliente>()
                .HasKey(c => c.ID_Cliente);

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithOne(u => u.Cliente)
                .HasForeignKey<Cliente>(c => c.Correo);
        }

    }
}
