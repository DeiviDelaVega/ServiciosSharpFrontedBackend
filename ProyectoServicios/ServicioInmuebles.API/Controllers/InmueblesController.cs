using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioInmuebles.API.Data;
using ServicioInmuebles.API.Models;
using System.Security.Claims;
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
        public async Task<ActionResult> Update(int id, [FromBody] Inmueble body)
        {
            var entity = await _ctx!.Inmueble.FindAsync(id);
            if (entity is null) return NotFound();

            entity.Nombre = body.Nombre;
            entity.Capacidad = body.Capacidad;
            entity.Numero_Habitaciones = body.Numero_Habitaciones;
            entity.Descripcion = body.Descripcion;
            entity.Servicios_Incluidos = body.Servicios_Incluidos;
            entity.Disponibilidad = body.Disponibilidad;
            entity.Precio_Por_Noche = body.Precio_Por_Noche;
            entity.Latitud = body.Latitud;
            entity.Longitud = body.Longitud;

            await _ctx.SaveChangesAsync();
            return NoContent();
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
