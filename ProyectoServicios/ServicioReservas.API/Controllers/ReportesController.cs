using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ServicioReservas.API.Data;
using Shared.Models;

namespace ServicioReservas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly ReservasDbContext _context;

        public ReportesController(ReservasDbContext context)
        {
            _context = context;
        }

        [HttpGet("InmueblesMasReservados")]
        public async Task<IActionResult> InmueblesMasReservados()
        {
            var resultado = await _context.Reserva // DbContext (acceso al DbSet<Reserva>)
                .GroupBy(r => r.ID_Inmueble)
                .Select(g => new InmueblesMasReservadosDto
                {
                    ID_Inmueble = g.Key,
                    CantidadReservas = g.Count()
                })
                .OrderByDescending(x => x.CantidadReservas)
                .Take(5) // Solo los 5 primeros
                .ToListAsync();

            return Ok(resultado);
        }

        [HttpGet("ClientesConMasReservas")]
        public async Task<IActionResult> ClientesConMasReservas()
        {
            var resultado = await _context.Reserva
                .GroupBy(r => r.ID_Cliente)
                .Select(g => new ClientesConMasReservasDto
                {
                    ID_Cliente = g.Key,
                    CantidadReservas = g.Count()
                })
                .OrderByDescending(x => x.CantidadReservas)
                .Take(5) // Solo los 5 clientes con más reservas
                .ToListAsync();

            return Ok(resultado);
        }
    }
}
