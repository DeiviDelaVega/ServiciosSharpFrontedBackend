using System.Security.Claims;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioInmuebles.API.Data;
using ServicioInmuebles.API.Service;
using Shared.Models;

namespace ServicioInmuebles.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    //[Authorize(Roles = "cliente")]
    [Route("api/cliente/catalogo")]
    public class CatalogoController : ControllerBase
    {
        private readonly InmueblesDbContext _context;
        private readonly ClienteService _clienteService;

        public CatalogoController(InmueblesDbContext context, ClienteService clienteService)
        {
            _context = context;
            _clienteService = clienteService;
        }

        // ===== MÉTODOS PRIVADOS =====

        private string GetCorreoUsuario()
        {
            var correo = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(correo))
                throw new UnauthorizedAccessException("Usuario no autenticado o sin claim de correo.");
            return correo;
        }

        private async Task<ClienteDto?> ObtenerClienteActualAsync()
        {
            try
            {
                var correo = GetCorreoUsuario();
                return await _clienteService.GetClientePorCorreoAsync(correo);
            }
            catch
            {
                return null;
            }
        }



        // ===== ENDPOINTS =====

        [HttpGet]
        public async Task<IActionResult> HomeCliente()
        {
            var cliente = await ObtenerClienteActualAsync();
            if (cliente == null)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "No se pudo obtener información del cliente.");

            var response = new CatalogoResponse
            {
                NombreCliente = $"{cliente.Nombre} {cliente.Apellido}"
            };

            if (cliente.Estado?.Equals("sancionado", StringComparison.OrdinalIgnoreCase) == true)
            {
                response.ModalSancion = true;
                response.Alerta = "Su cuenta ha sido sancionada por infringir las normas del sistema.";
            }

            return Ok(response);
        }

        [HttpGet("ListarInmuebles")]
        public async Task<IActionResult> ListarInmuebles(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 6,
    [FromQuery] string filtro = "",
    [FromQuery] decimal? precioDesde = null,
    [FromQuery] decimal? precioHasta = null,
    [FromQuery] string estado = "")
        {
            var cliente = await ObtenerClienteActualAsync();
            if (cliente == null)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "No se pudo obtener información del cliente.");

            var response = new CatalogoResponse
            {
                NombreCliente = $"{cliente.Nombre} {cliente.Apellido}"
            };

            if (cliente.Estado?.Equals("sancionado", StringComparison.OrdinalIgnoreCase) == true)
            {
                response.ModalSancion = true;
                response.Alerta = "Su cuenta ha sido sancionada por infringir las normas del sistema.";
                return Ok(response); // devolvemos la alerta sin inmuebles
            }

            var query = _context.Inmueble.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                var filtroLower = filtro.ToLower();
                query = query.Where(i =>
                    i.Nombre.ToLower().Contains(filtroLower) ||
                    i.Descripcion.ToLower().Contains(filtroLower) ||
                    i.ServiciosIncluidos.ToLower().Contains(filtroLower));
            }

            if (precioDesde.HasValue)
                query = query.Where(i => i.PrecioPorNoche >= precioDesde.Value);

            if (precioHasta.HasValue)
                query = query.Where(i => i.PrecioPorNoche <= precioHasta.Value);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(i => i.Disponibilidad.ToLower() == estado.ToLower());

            var totalRegistros = await query.CountAsync();
            var inmuebles = await query
                .OrderBy(i => i.IdInmueble)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            response.Inmuebles = inmuebles.Select(i => new InmuebleDto
            {
                IdInmueble = i.IdInmueble,
                Nombre = i.Nombre,
                Capacidad = i.Capacidad,
                NumeroHabitaciones = i.NumeroHabitaciones,
                Descripcion = i.Descripcion,
                ServiciosIncluidos = i.ServiciosIncluidos,
                Disponibilidad = i.Disponibilidad,
                PrecioPorNoche = i.PrecioPorNoche,
                ImagenHabitacion = i.ImagenHabitacion,
                Latitud = i.Latitud,
                Longitud = i.Longitud
            }).ToList();

            response.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize);
            response.PaginaActual = pageNumber;

            if (!inmuebles.Any())
                response.Alerta = "No se encontraron inmuebles con los filtros ingresados.";

            return Ok(response);
        }




        [HttpGet("detalle/{id}")]
        public async Task<IActionResult> VerDetalleInmueble(int id)
        {
            var inmueble = await _context.Inmueble.FindAsync(id);
            if (inmueble == null) return NotFound();

            return Ok(new InmuebleDto
            {
                IdInmueble = inmueble.IdInmueble,
                Nombre = inmueble.Nombre,
                Capacidad = inmueble.Capacidad,
                NumeroHabitaciones = inmueble.NumeroHabitaciones,
                Descripcion = inmueble.Descripcion,
                ServiciosIncluidos = inmueble.ServiciosIncluidos,
                Disponibilidad = inmueble.Disponibilidad,
                PrecioPorNoche = inmueble.PrecioPorNoche,
                ImagenHabitacion = inmueble.ImagenHabitacion,
                Latitud = inmueble.Latitud,
                Longitud = inmueble.Longitud
            });
        }

        [HttpGet("ocupadas/{idInmueble}")]
        public async Task<IActionResult> ObtenerFechasOcupadas(int idInmueble)
        {
            var reservas = await _context.Reservas
                .Where(r => r.ID_Inmueble == idInmueble)
                .Select(r => new { r.Fecha_Inicio_Reserva, r.Fecha_Fin_Reserva })
                .ToListAsync();

            return Ok(reservas);
        }

        [HttpGet("terminos")]
        public IActionResult MostrarTerminos() =>
            Ok("Aquí iría el texto de los términos y condiciones");

        [HttpGet("motivoSancion")]
        public IActionResult VerMotivoSancion() =>
            Ok("Motivo de sanción: incumplimiento de normas del servicio");
    }
}
