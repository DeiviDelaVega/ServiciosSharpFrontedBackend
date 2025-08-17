using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioReservas.API.Data;
using ServicioReservas.API.Models;
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

        private async Task<bool> HayTraslapeAsync(int idInmueble, DateTime ini, DateTime fin)
        {
            return await _context.Reserva.AnyAsync(r =>
                r.ID_Inmueble == idInmueble &&
                (r.Estado_Reserva == "Solicitado" || r.Estado_Reserva == "Aprobado") &&
                ini < r.Fecha_Fin_Reserva && fin > r.Fecha_Inicio_Reserva);
        }

        [HttpGet("ocupadas/{inmuebleId:int}")]
        public async Task<ActionResult<List<string>>> GetFechasOcupadas(int inmuebleId)
        {
            var reservas = await _context.Reserva
                .Where(r => r.ID_Inmueble == inmuebleId &&
                       (r.Estado_Reserva == "Solicitado" || r.Estado_Reserva == "Aprobado"))
                .Select(r => new { r.Fecha_Inicio_Reserva, r.Fecha_Fin_Reserva })
                .ToListAsync();

            var ocupadas = new List<string>();
            foreach (var r in reservas)
            {
                for (var d = r.Fecha_Inicio_Reserva.Date; d < r.Fecha_Fin_Reserva.Date; d = d.AddDays(1))
                    ocupadas.Add(d.ToString("yyyy-MM-dd"));
            }
            return ocupadas.Distinct().ToList();
        }

        [HttpPost]
        public async Task<ActionResult> Confirmar([FromBody] ConfirmarReservaDto dto)
        {
            if (dto.FechaFin <= dto.FechaInicio)
                return BadRequest("Rango de fechas inválido.");

            if (await HayTraslapeAsync(dto.IdInmueble, dto.FechaInicio, dto.FechaFin))
                return Conflict("El inmueble ya está reservado en ese rango.");

            using var tx = await _context.Database.BeginTransactionAsync();

            var reserva = new Reserva
            {
                // 🔴 agrega esta línea:
                Fecha_Solicitud = DateTime.Now, // o DateTime.UtcNow

                ID_Cliente = dto.IdCliente,
                ID_Inmueble = dto.IdInmueble,
                Fecha_Inicio_Reserva = dto.FechaInicio,
                Fecha_Fin_Reserva = dto.FechaFin,
                Monto_Total = dto.MontoTotal,
                Metodo_Pago = "Tarjeta",       // opcional, pero explícito
                Estado_Reserva = "Aprobado"
            };

            _context.Reserva.Add(reserva);
            await _context.SaveChangesAsync();

            var pago = new Pago
            {
                ID_Solicitud = reserva.ID_Solicitud,
                Monto = dto.MontoTotal,
                Stripe_Payment_Id = dto.StripePaymentId
            };

            _context.Pago.Add(pago);
            await _context.SaveChangesAsync();

            await tx.CommitAsync();
            return Ok(new { reserva.ID_Solicitud });
        }





    }
}