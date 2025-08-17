using Microsoft.EntityFrameworkCore;
using Shared.Models;
using ServicioInmuebles.API.Models;

namespace ServicioInmuebles.API.Data
{
    public class InmueblesDbContext : DbContext
    {
        public InmueblesDbContext(DbContextOptions<InmueblesDbContext> options) : base(options) { }

        public DbSet<ClienteDto> Clientes { get; set; }
        public DbSet<InmuebleDto> Inmueble { get; set; }
        public DbSet<ReservaDto> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // Cliente
            mb.Entity<ClienteDto>().HasKey(c => c.IdCliente);

            // Inmueble
            mb.Entity<InmuebleDto>().HasKey(i => i.IdInmueble);
            mb.Entity<InmuebleDto>().ToTable("Inmueble");

            mb.Entity<InmuebleDto>().Property(i => i.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            mb.Entity<InmuebleDto>().Property(i => i.Descripcion)
                .HasMaxLength(300);

            mb.Entity<InmuebleDto>().Property(i => i.ServiciosIncluidos)
                .HasMaxLength(200);

            mb.Entity<InmuebleDto>().Property(i => i.Disponibilidad)
                .HasMaxLength(2);

            mb.Entity<InmuebleDto>().Property(i => i.PrecioPorNoche)
                .HasColumnType("decimal(10,2)");

            mb.Entity<InmuebleDto>().Property(i => i.Latitud)
                .HasColumnType("decimal(10,8)");

            mb.Entity<InmuebleDto>().Property(i => i.Longitud)
                .HasColumnType("decimal(11,8)");

            // Reserva
            mb.Entity<ReservaDto>().HasKey(r => r.ID_Solicitud);

            mb.Entity<ReservaDto>()
                .HasOne(r => r.Cliente)
                .WithMany()
                .HasForeignKey(r => r.ID_Cliente)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<ReservaDto>()
                .HasOne(r => r.Inmueble)
                .WithMany()
                .HasForeignKey(r => r.ID_Inmueble)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
