using Microsoft.EntityFrameworkCore;
using ServicioReservas.API.Models;
namespace ServicioReservas.API.Data
{
    public class ReservasDbContext : DbContext
    {
        public ReservasDbContext(DbContextOptions<ReservasDbContext> options) : base(options) { }

        public DbSet<Reserva> Reserva { get; set; }
        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<Inmueble> Inmueble { get; set; }
        // Agrega aquí los DbSet necesarios
    }
}