using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioReservas.API.Data;
using Shared.Models;

namespace ServicioReservas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservaController : ControllerBase
    {
        private readonly ReservasDbContext _context;

        public ReservaController(ReservasDbContext context)
        {
            _context = context;
        }

        // GET: api/Reserva
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservaDto>>> GetReservas(
            [FromQuery] int page = 0,
            [FromQuery] int size = 5)
        {
            var query = _context.Reserva.AsQueryable();

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)size);

            var reservas = await query
                .Skip(page * size)
                .Take(size)
                .Select(r => new ReservaDto
                {
                    ID_Solicitud = r.ID_Solicitud,
                    Fecha_Solicitud = r.Fecha_Solicitud,
                    ID_Cliente = r.ID_Cliente,
                    ID_Inmueble = r.ID_Inmueble,
                    Fecha_Inicio_Reserva = r.Fecha_Inicio_Reserva,
                    Fecha_Fin_Reserva = r.Fecha_Fin_Reserva,
                    Metodo_Pago = r.Metodo_Pago,
                    Monto_Total = r.Monto_Total,
                    Estado_Reserva = r.Estado_Reserva
                    // No incluyas Cliente ni Inmueble aquí
                })
                .ToListAsync();

            return Ok(new
            {
                reservas,
                currentPage = page,
                totalPages,
                totalItems
            });
        }

        // GET: api/Reserva/filtrar-estado
        [HttpGet("filtrar-estado")]
        public async Task<IActionResult> FiltrarPorEstado(
            [FromQuery] string estado,
            [FromQuery] int page = 0,
            [FromQuery] int size = 5)
        {
            var query = _context.Reserva
                .Where(r => r.Estado_Reserva == estado)
                .AsQueryable();

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)size);

            var reservas = await query
                .Skip(page * size)
                .Take(size)
                .Select(r => new ReservaDto
                {
                    ID_Solicitud = r.ID_Solicitud,
                    Fecha_Solicitud = r.Fecha_Solicitud,
                    ID_Cliente = r.ID_Cliente,
                    ID_Inmueble = r.ID_Inmueble,
                    Fecha_Inicio_Reserva = r.Fecha_Inicio_Reserva,
                    Fecha_Fin_Reserva = r.Fecha_Fin_Reserva,
                    Metodo_Pago = r.Metodo_Pago,
                    Monto_Total = r.Monto_Total,
                    Estado_Reserva = r.Estado_Reserva
                })
                .ToListAsync();

            return Ok(new
            {
                reservas,
                currentPage = page,
                totalPages,
                totalItems
            });
        }

        // GET: api/Reserva/filtrar-fechas
        [HttpGet("filtrar-fechas")]
        public async Task<IActionResult> FiltrarPorFechas(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin,
            [FromQuery] int page = 0,
            [FromQuery] int size = 5)
        {
            var query = _context.Reserva
                .Where(r => r.Fecha_Inicio_Reserva >= fechaInicio && r.Fecha_Fin_Reserva <= fechaFin)
                .AsQueryable();

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)size);

            var reservas = await query
                .Skip(page * size)
                .Take(size)
                .Select(r => new ReservaDto
                {
                    ID_Solicitud = r.ID_Solicitud,
                    Fecha_Solicitud = r.Fecha_Solicitud,
                    ID_Cliente = r.ID_Cliente,
                    ID_Inmueble = r.ID_Inmueble,
                    Fecha_Inicio_Reserva = r.Fecha_Inicio_Reserva,
                    Fecha_Fin_Reserva = r.Fecha_Fin_Reserva,
                    Metodo_Pago = r.Metodo_Pago,
                    Monto_Total = r.Monto_Total,
                    Estado_Reserva = r.Estado_Reserva
                })
                .ToListAsync();

            return Ok(new
            {
                reservas,
                currentPage = page,
                totalPages,
                totalItems
            });
        }

        // PUT: api/Reserva/{id}/estado
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromQuery] string estado)
        {
            var reserva = await _context.Reserva.FindAsync(id);
            if (reserva == null)
                return NotFound(new { mensaje = "Reserva no encontrada" });

            reserva.Estado_Reserva = estado;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Estado actualizado correctamente" });
        }

        // DELETE: api/Reserva/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarReserva(int id)
        {
            var reserva = await _context.Reserva.FindAsync(id);
            if (reserva == null)
                return NotFound(new { mensaje = "Reserva no encontrada" });

            _context.Reserva.Remove(reserva);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Reserva eliminada correctamente" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReserva(int id)
        {
            var reserva = await _context.Reserva.FindAsync(id);
            if (reserva == null)
                return NotFound();
            return Ok(reserva);
        }
    }
}