using System.Security.Claims;
using System.Text.RegularExpressions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioInmuebles.API.Data;
using ServicioInmuebles.API.Models;
using Shared.Models;

namespace ServicioInmuebles.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InmueblesController : Controller
    {
        private readonly InmueblesDbContext? _ctx;
        private readonly IConfiguration? _config;

        public InmueblesController(InmueblesDbContext ctx, IConfiguration config)
        {
            _ctx = ctx;
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InmuebleDto>>> GetAll()
            => await _ctx!.Inmueble.OrderByDescending(i => i.IdInmueble).ToListAsync();

        [HttpGet("{id:int}")]
        public async Task<ActionResult<InmuebleDto>> Get(int id)
        {
            var x = await _ctx!.Inmueble.FindAsync(id);
            return x is null ? NotFound() : x;
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [RequestSizeLimit(20_000_000)]
        public async Task<ActionResult> Create([FromForm] CrearInmuebleDto dto, IFormFile? imagen)
        {
            var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? adminId = int.TryParse(adminIdClaim, out var parsed) ? parsed : null;

            string? urlImagen = null;
            if (imagen != null && imagen.Length > 0)
            {
                var acc = new Account(
                    _config!["Cloudinary:CloudName"],
                    _config["Cloudinary:ApiKey"],
                    _config["Cloudinary:ApiSecret"]);

                var cloud = new Cloudinary(acc);
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(imagen.FileName, imagen.OpenReadStream()),
                    Folder = "inmuebles"
                };
                var result = await cloud.UploadAsync(uploadParams);
                urlImagen = result.SecureUrl?.ToString();
            }

            var entity = new InmuebleDto
            {
                Nombre = dto.Nombre,
                Capacidad = dto.Capacidad,
                NumeroHabitaciones = dto.Numero_Habitaciones,
                Descripcion = dto.Descripcion,
                ServiciosIncluidos = dto.Servicios_Incluidos,
                Disponibilidad = dto.Disponibilidad,
                PrecioPorNoche = dto.Precio_Por_Noche,
                Latitud = dto.Latitud,
                Longitud = dto.Longitud,
                ImagenHabitacion = urlImagen,
                ID_Admin_Creador = adminId
            };

            _ctx!.Inmueble.Add(entity);
            await _ctx.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = entity.IdInmueble }, entity);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20_000_000)]
        public async Task<ActionResult<InmuebleDto>> Update(int id, [FromForm] CrearInmuebleDto dto, IFormFile? imagen)
        {
            var entity = await _ctx!.Inmueble.FindAsync(id);
            if (entity is null) return NotFound();

            entity.Nombre = dto.Nombre;
            entity.Capacidad = dto.Capacidad;
            entity.NumeroHabitaciones = dto.Numero_Habitaciones;
            entity.Descripcion = dto.Descripcion;
            entity.ServiciosIncluidos = dto.Servicios_Incluidos;
            entity.Disponibilidad = dto.Disponibilidad;
            entity.PrecioPorNoche = dto.Precio_Por_Noche;
            entity.Latitud = dto.Latitud;
            entity.Longitud = dto.Longitud;

            if (imagen != null && imagen.Length > 0)
            {
                var acc = new Account(_config!["Cloudinary:CloudName"], _config["Cloudinary:ApiKey"], _config["Cloudinary:ApiSecret"]);
                var cloud = new Cloudinary(acc);

                if (!string.IsNullOrEmpty(entity.ImagenHabitacion))
                {
                    var publicId = ExtraerPublicIdDesdeUrl(entity.ImagenHabitacion);
                    if (!string.IsNullOrEmpty(publicId))
                        await cloud.DestroyAsync(new DeletionParams(publicId) { ResourceType = ResourceType.Image });
                }

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(imagen.FileName, imagen.OpenReadStream()),
                    Folder = "inmuebles",
                    PublicId = $"inmueble_{id}_{Guid.NewGuid()}"
                };
                var result = await cloud.UploadAsync(uploadParams);
                entity.ImagenHabitacion = result.SecureUrl?.ToString();
            }

            await _ctx.SaveChangesAsync();
            return Ok(entity);
        }

        private string? ExtraerPublicIdDesdeUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            var regex = new Regex(@"/upload/(?:v\d+/)?(.+)\.\w+$");
            var match = regex.Match(url);

            if (match.Success && match.Groups.Count > 1)
                return match.Groups[1].Value;

            return null;
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var entity = await _ctx!.Inmueble.FindAsync(id);
            if (entity is null) return NotFound();

            _ctx.Remove(entity);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        // Para Reporte
        [HttpGet("Obtener/{id}")]
        public IActionResult GetInmueble(int id)
        {
            var inmueble = _ctx.Inmueble
                .Where(i => i.IdInmueble == id)
                .Select(i => new InmuebleDto
                {
                    IdInmueble = i.IdInmueble,
                    Nombre = i.Nombre
                })
                .FirstOrDefault();

            if (inmueble == null) return NotFound();
            return Ok(inmueble);
        }
    }
}
