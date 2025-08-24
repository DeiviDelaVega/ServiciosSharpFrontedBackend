using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioReservas.API.Data;
using ServicioReservas.API.Models;
using Shared.Models; // <- DTOs compartidos
using System.Net.Http.Json;
using System.Security.Claims;

namespace ServicioReservas.API.Controllers
{
    [ApiController]
    [Route("api/cliente/misreservas")]
    [Authorize(Roles = "cliente")]
    public class MisReservasController : ControllerBase
    {
        private readonly ReservasDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public MisReservasController(
          ReservasDbContext context,
          IHttpClientFactory httpClientFactory,
          EmailService emailService,
          IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _emailService = emailService;
            _config = config;
        }

        // GET: /api/cliente/misreservas?page=0&size=5
        [HttpGet]
        public async Task<IActionResult> ListarMisReservas([FromQuery] int page = 1, [FromQuery] int size = 5)
        {
            var idClienteClaim = User.FindFirstValue("ID_Cliente");
            if (string.IsNullOrWhiteSpace(idClienteClaim))
                return Unauthorized(new { error = "No autenticado" });

            if (!int.TryParse(idClienteClaim, out var idCliente))
                return Unauthorized(new { error = "ID de cliente inválido" });

            // Datos del cliente (microservicio clientes)
            var clientesHttp = _httpClientFactory.CreateClient("ServicioClientes");
            var cliResp = await clientesHttp.GetAsync($"api/admin/Cliente/{idCliente}");
            if (!cliResp.IsSuccessStatusCode)
                return NotFound(new { error = "Cliente no encontrado" });

            var clienteDto = await cliResp.Content.ReadFromJsonAsync<ClienteDto>();
            if (clienteDto is null)
                return NotFound(new { error = "Cliente no encontrado" });

            bool esActivo = clienteDto.Estado?.Equals("activo", StringComparison.OrdinalIgnoreCase) == true;
            bool modalSancion = clienteDto.Estado?.Equals("sancionado", StringComparison.OrdinalIgnoreCase) == true;

            // Query paginada de reservas del cliente
            var query = _context.Reserva
                .Where(r => r.ID_Cliente == idCliente)
                .OrderByDescending(r => r.Fecha_Solicitud);

            var totalElementos = await query.CountAsync();

            if (size <= 1) size = 5;
            

            if (page < 1) page = 1;

            var reservas = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            // Enriquecer con datos de inmueble y flag de reembolso
            var inmueblesHttp = _httpClientFactory.CreateClient("ServicioInmuebles");
            var reservasDto = new List<ReservaDto>();

            foreach (var r in reservas)
            {
                InmuebleDto? inmuebleDto = null;
                try
                {
                    var inmResp = await inmueblesHttp.GetAsync($"api/Inmuebles/{r.ID_Inmueble}");
                    if (inmResp.IsSuccessStatusCode)
                        inmuebleDto = await inmResp.Content.ReadFromJsonAsync<InmuebleDto>();
                }
                catch { /* ignorar y dejar inmuebleDto = null */ }

                // Buscar último pago (para usar su fecha como base)
                var pago = await _context.Pago
                    .Where(p => p.ID_Solicitud == r.ID_Solicitud)
                    .OrderByDescending(p => p.Fecha_Pago)
                    .FirstOrDefaultAsync();

                // Base de tiempo: si hay pago usamos Fecha_Pago; si no, Fecha_Solicitud
                var baseTime = pago?.Fecha_Pago ?? r.Fecha_Solicitud;
                var diff = DateTime.Now - baseTime;

                bool reembolsable = diff.TotalMinutes <= 1;
                long? msRestantes = null;
                if (reembolsable)
                {
                    var restante = TimeSpan.FromMinutes(1) - diff;
                    msRestantes = (long)Math.Max(0, restante.TotalMilliseconds);
                }

                reservasDto.Add(new ReservaDto
                {
                    ID_Solicitud = r.ID_Solicitud,
                    Fecha_Solicitud = r.Fecha_Solicitud,
                    ID_Cliente = r.ID_Cliente,
                    ID_Inmueble = r.ID_Inmueble,
                    Fecha_Inicio_Reserva = r.Fecha_Inicio_Reserva,
                    Fecha_Fin_Reserva = r.Fecha_Fin_Reserva,
                    Metodo_Pago = r.Metodo_Pago,
                    Monto_Total = r.Monto_Total,
                    Estado_Reserva = r.Estado_Reserva,
                    Cliente = clienteDto,
                    Inmueble = inmuebleDto,

                    // ✅ Flags para el front
                    Reembolsable = reembolsable,
                    TiempoRestanteReembolsoMs = msRestantes
                });
            }

            var totalPaginas = (int)Math.Ceiling(totalElementos / (double)size);

            var pageInfo = new
            {
                hasPrevious = page > 1,
                hasNext = page < totalPaginas,
                paginaActual = page,   // <- directo
                totalPaginas,
                first = page == 1,
                last = page == totalPaginas,
                totalElementos,
                size
            };



            return Ok(new
            {
                esActivo,
                modalSancion,
                alerta = modalSancion ? "Su cuenta ha sido sancionada. Comuníquese con administración." : null,
                nombreCliente = clienteDto.Nombre,
                cliente = new
                {
                    id = clienteDto.IdCliente,
                    nombre = clienteDto.Nombre,
                    apellido = clienteDto.Apellido,
                    correo = clienteDto.Correo,
                    estado = clienteDto.Estado
                },
                reservas = reservasDto,
                page = pageInfo
            });
        }

        // GET: /api/cliente/misreservas/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> DetalleReservaCliente(int id)
        {
            var correo = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(correo))
                return Unauthorized("No autenticado");

            var reserva = await _context.Reserva.FirstOrDefaultAsync(r => r.ID_Solicitud == id);
            if (reserva == null) return NotFound("No encontrado");

            // Validar pertenencia
            var clientesHttp = _httpClientFactory.CreateClient("ServicioClientes");
            var cliResp = await clientesHttp.GetAsync($"api/admin/Cliente/{reserva.ID_Cliente}");
            if (!cliResp.IsSuccessStatusCode) return Forbid("No autorizado");

            var clienteDto = await cliResp.Content.ReadFromJsonAsync<ClienteDto>();
            if (clienteDto == null || !string.Equals(clienteDto.Correo, correo, StringComparison.OrdinalIgnoreCase))
                return Forbid("No autorizado");

            InmuebleDto? inmuebleDto = null;
            try
            {
                var inmueblesHttp = _httpClientFactory.CreateClient("ServicioInmuebles");
                var inmResp = await inmueblesHttp.GetAsync($"api/Inmuebles/{reserva.ID_Inmueble}");
                if (inmResp.IsSuccessStatusCode)
                    inmuebleDto = await inmResp.Content.ReadFromJsonAsync<InmuebleDto>();
            }
            catch { }

            var pago = await _context.Pago
                .Where(p => p.ID_Solicitud == reserva.ID_Solicitud)
                .OrderByDescending(p => p.Fecha_Pago)
                .FirstOrDefaultAsync();

            var baseTime = pago?.Fecha_Pago ?? reserva.Fecha_Solicitud;
            var diff = DateTime.Now - baseTime;

            bool reembolsable = diff.TotalMinutes <= 1;
            long? msRestantes = null;
            if (reembolsable)
            {
                var restante = TimeSpan.FromMinutes(1) - diff;
                msRestantes = (long)Math.Max(0, restante.TotalMilliseconds);
            }

            var dto = new ReservaDto
            {
                ID_Solicitud = reserva.ID_Solicitud,
                Fecha_Solicitud = reserva.Fecha_Solicitud,
                ID_Cliente = reserva.ID_Cliente,
                ID_Inmueble = reserva.ID_Inmueble,
                Fecha_Inicio_Reserva = reserva.Fecha_Inicio_Reserva,
                Fecha_Fin_Reserva = reserva.Fecha_Fin_Reserva,
                Metodo_Pago = reserva.Metodo_Pago,
                Monto_Total = reserva.Monto_Total,
                Estado_Reserva = reserva.Estado_Reserva,
                Cliente = clienteDto,
                Inmueble = inmuebleDto,
                Reembolsable = reembolsable,
                TiempoRestanteReembolsoMs = msRestantes
            };

            return Ok(dto);
        }

        // DELETE: /api/cliente/misreservas/{id}/reembolso
        [HttpDelete("{id:int}/reembolso")]
        public async Task<IActionResult> ReembolsoDelete(int id)
        {
            var correo = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(correo))
                return Unauthorized("No autenticado");

            var reserva = await _context.Reserva.FirstOrDefaultAsync(r => r.ID_Solicitud == id);
            if (reserva == null) return NotFound("No encontrado");

            // Validar pertenencia
            var clientesHttp = _httpClientFactory.CreateClient("ServicioClientes");
            var cliResp = await clientesHttp.GetAsync($"api/admin/Cliente/{reserva.ID_Cliente}");
            if (!cliResp.IsSuccessStatusCode) return Forbid("No autorizado");

            var clienteDto = await cliResp.Content.ReadFromJsonAsync<ClienteDto>();
            if (clienteDto == null || !string.Equals(clienteDto.Correo, correo, StringComparison.OrdinalIgnoreCase))
                return Forbid("No autorizado");

            // Debe existir pago
            var pago = await _context.Pago.FirstOrDefaultAsync(p => p.ID_Solicitud == reserva.ID_Solicitud);
            if (pago == null)
                return BadRequest("La reserva no tiene un pago registrado.");

            // Validar ventana de 1 minuto
            if ((DateTime.Now - pago.Fecha_Pago).TotalMinutes > 1)
                return BadRequest("El tiempo para reembolso ha expirado.");

            // Eliminar Pago y Reserva
            _context.Pago.Remove(pago);
            _context.Reserva.Remove(reserva);
            await _context.SaveChangesAsync();

            // ✅ Enviar correo al admin
            string adminEmail = _config["EmailSettings:AdminEmail"];
            string subject = $"🔔 Reembolso realizado - Reserva #{reserva.ID_Solicitud}";
            string body = $@"
                <h2>Detalle de Reembolso</h2>
                <p><b>Cliente:</b> {clienteDto.Nombre} {clienteDto.Apellido}</p>
                <p><b>Correo Cliente:</b> {clienteDto.Correo}</p>
                <p><b>Inmueble ID:</b> {reserva.ID_Inmueble}</p>
                <p><b>Fecha Inicio:</b> {reserva.Fecha_Inicio_Reserva}</p>
                <p><b>Fecha Fin:</b> {reserva.Fecha_Fin_Reserva}</p>
                <p><b>Monto Total:</b> S/. {reserva.Monto_Total}</p>
                <p><b>Estado:</b> REEMBOLSADO</p>
                <p><b>Fecha Solicitud:</b> {reserva.Fecha_Solicitud}</p>
            ";

            try
            {
                await _emailService.SendEmailAsync(adminEmail, subject, body);
            }
            catch (Exception ex)
            {
                // No interrumpimos el flujo si falla el correo
                Console.WriteLine($"Error enviando correo: {ex.Message}");
            }

            return Ok("OK");
        }
    }
}
