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
        public async Task<ActionResult<IEnumerable<Inmueble>>> GetAll()
            => await _ctx!.Inmueble.OrderByDescending(i => i.ID_Inmueble).ToListAsync();

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Inmueble>> Get(int id)
        {
            var x = await _ctx!.Inmueble.FindAsync(id);
            return x is null ? NotFound() : x;
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [RequestSizeLimit(20_000_000)]
        public async Task<ActionResult> Create([FromForm] CrearInmuebleDto dto, IFormFile? imagen)
        {
            // ID del admin (desde el JWT)
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

            var entity = new Inmueble
            {
                Nombre = dto.Nombre,
                Capacidad = dto.Capacidad,
                Numero_Habitaciones = dto.Numero_Habitaciones,
                Descripcion = dto.Descripcion,
                Servicios_Incluidos = dto.Servicios_Incluidos,
                Disponibilidad = dto.Disponibilidad,
                Precio_Por_Noche = dto.Precio_Por_Noche,
                Latitud = dto.Latitud,
                Longitud = dto.Longitud,
                Imagen_Habitacion = urlImagen,

                ID_Admin_Creador = adminId
            };

            _ctx!.Inmueble.Add(entity);
            await _ctx.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = entity.ID_Inmueble }, entity);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20_000_000)]
        public async Task<ActionResult<Inmueble>> Update(int id, [FromForm] CrearInmuebleDto dto, IFormFile? imagen)
        {
            var entity = await _ctx!.Inmueble.FindAsync(id);
            if (entity is null) return NotFound();

            // Actualiza campos básicos
            entity.Nombre = dto.Nombre;
            entity.Capacidad = dto.Capacidad;
            entity.Numero_Habitaciones = dto.Numero_Habitaciones;
            entity.Descripcion = dto.Descripcion;
            entity.Servicios_Incluidos = dto.Servicios_Incluidos;
            entity.Disponibilidad = dto.Disponibilidad;
            entity.Precio_Por_Noche = dto.Precio_Por_Noche;
            entity.Latitud = dto.Latitud;
            entity.Longitud = dto.Longitud;

            // Si llegó una nueva imagen, borra la anterior y sube la nueva
            if (imagen != null && imagen.Length > 0)
            {
                var acc = new Account(_config!["Cloudinary:CloudName"], _config["Cloudinary:ApiKey"], _config["Cloudinary:ApiSecret"]);
                var cloud = new Cloudinary(acc);

                if (!string.IsNullOrEmpty(entity.Imagen_Habitacion))
                {
                    var publicId = ExtraerPublicIdDesdeUrl(entity.Imagen_Habitacion);
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
                entity.Imagen_Habitacion = result.SecureUrl?.ToString();
            }

            await _ctx.SaveChangesAsync();
            return Ok(entity);
        }

        // Método para extraer publicId desde URL
        private string? ExtraerPublicIdDesdeUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            // Patrón: tomar todo después de "/upload/v123456/" y quitar la extensión
            var regex = new Regex(@"/upload/(?:v\d+/)?(.+)\.\w+$");
            var match = regex.Match(url);

            if (match.Success && match.Groups.Count > 1)
                return match.Groups[1].Value; // ej. inmuebles/ns7bvyerbafzvdjeqm8g

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

    }
}
