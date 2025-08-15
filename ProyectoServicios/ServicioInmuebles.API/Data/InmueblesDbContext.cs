using Microsoft.EntityFrameworkCore;
using ServicioInmuebles.API.Models;

namespace ServicioInmuebles.API.Data
{
    public class InmueblesDbContext : DbContext
    {
        public InmueblesDbContext(DbContextOptions<InmueblesDbContext> options) : base(options) { }

        public DbSet<Inmueble> Inmueble { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Inmueble>().HasKey(i => i.ID_Inmueble);

            mb.Entity<Inmueble>()
              .Property(i => i.Nombre)
              .IsRequired()
              .HasMaxLength(100);

            mb.Entity<Inmueble>().Property(i => i.Precio_Por_Noche)
            .HasColumnType("decimal(10,2)");

            mb.Entity<Inmueble>().Property(i => i.Latitud)
                .HasColumnType("decimal(10,8)");

            mb.Entity<Inmueble>().Property(i => i.Longitud)
                .HasColumnType("decimal(11,8)");

            mb.Entity<Inmueble>().Property(i => i.Descripcion).HasMaxLength(300);
            mb.Entity<Inmueble>().Property(i => i.Servicios_Incluidos).HasMaxLength(200);
            mb.Entity<Inmueble>().Property(i => i.Disponibilidad).HasMaxLength(2);

        }
    }
}
