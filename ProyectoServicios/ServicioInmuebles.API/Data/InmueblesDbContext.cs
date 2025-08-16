using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using ServicioInmuebles.API.Models;

namespace ServicioInmuebles.API.Data
{
    public class InmueblesDbContext : DbContext
    {
        public InmueblesDbContext(DbContextOptions<InmueblesDbContext> options): base(options) {}
        public InmueblesDbContext(DbContextOptions<InmueblesDbContext> options) : base(options) { }

        public DbSet<ClienteDto> Clientes { get; set; }
        public DbSet<InmuebleDto> Inmueble { get; set; }
        public DbSet<ReservaDto> Reservas { get; set; }
        public DbSet<Inmueble> Inmueble { get; set; }

        
        protected override void OnModelCreating(ModelBuilder mb)
        {
            modelBuilder.Entity<ClienteDto>().HasKey(c => c.IdCliente);
            modelBuilder.Entity<InmuebleDto>().HasKey(i => i.IdInmueble);
            mb.Entity<Inmueble>().HasKey(i => i.ID_Inmueble);

            mb.Entity<Inmueble>()
              .Property(i => i.Nombre)
              .IsRequired()
              .HasMaxLength(100);

            mb.Entity<Inmueble>().Property(i => i.Precio_Por_Noche)
            .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<InmuebleDto>().ToTable("Inmueble");
            mb.Entity<Inmueble>().Property(i => i.Latitud)
                .HasColumnType("decimal(10,8)");

            modelBuilder.Entity<ReservaDto>().HasKey(r => r.ID_Solicitud);
            mb.Entity<Inmueble>().Property(i => i.Longitud)
                .HasColumnType("decimal(11,8)");

            // Relaciones opcionales para EF (si quieres navegar desde reservas a cliente/inmueble)
            modelBuilder.Entity<ReservaDto>()
                .HasOne(r => r.Cliente)
                .WithMany()
                .HasForeignKey(r => r.ID_Cliente)
                .OnDelete(DeleteBehavior.Restrict);
            mb.Entity<Inmueble>().Property(i => i.Descripcion).HasMaxLength(300);
            mb.Entity<Inmueble>().Property(i => i.Servicios_Incluidos).HasMaxLength(200);
            mb.Entity<Inmueble>().Property(i => i.Disponibilidad).HasMaxLength(2);

            modelBuilder.Entity<ReservaDto>()
                .HasOne(r => r.Inmueble)
                .WithMany()
                .HasForeignKey(r => r.ID_Inmueble)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

