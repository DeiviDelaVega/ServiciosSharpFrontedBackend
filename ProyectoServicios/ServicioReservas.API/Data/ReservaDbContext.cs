using Microsoft.EntityFrameworkCore;
using ServicioReservas.API.Models;

namespace ServicioReservas.API.Data
{
    public class ReservasDbContext : DbContext
    {
        public ReservasDbContext(DbContextOptions<ReservasDbContext> options) : base(options) { }

        public DbSet<Reserva> Reserva { get; set; }
        public DbSet<Pago> Pago { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Reserva>(e =>
            {
                e.ToTable("Reserva");
                e.HasKey(x => x.ID_Solicitud);
                e.Property(x => x.ID_Solicitud).HasColumnName("ID_Solicitud");
                e.Property(x => x.Fecha_Solicitud).HasColumnName("Fecha_Solicitud");
                e.Property(x => x.ID_Cliente).HasColumnName("ID_Cliente");
                e.Property(x => x.ID_Inmueble).HasColumnName("ID_Inmueble");
                e.Property(x => x.Fecha_Inicio_Reserva).HasColumnName("Fecha_Inicio_Reserva");
                e.Property(x => x.Fecha_Fin_Reserva).HasColumnName("Fecha_Fin_Reserva");
                e.Property(x => x.Metodo_Pago).HasColumnName("Metodo_Pago");
                e.Property(x => x.Monto_Total).HasColumnName("Monto_Total");
                e.Property(x => x.Estado_Reserva).HasColumnName("Estado_Reserva");
            });

            mb.Entity<Pago>(e =>
            {
                e.ToTable("Pago");
                e.HasKey(x => x.ID_Pago);
                e.Property(x => x.ID_Pago).HasColumnName("ID_Pago");
                e.Property(x => x.ID_Solicitud).HasColumnName("ID_Solicitud");
                e.Property(x => x.Fecha_Pago).HasColumnName("Fecha_Pago");
                e.Property(x => x.Monto).HasColumnName("Monto");
                e.Property(x => x.Stripe_Payment_Id).HasColumnName("Stripe_Payment_Id");
            });
        }
    }
}
