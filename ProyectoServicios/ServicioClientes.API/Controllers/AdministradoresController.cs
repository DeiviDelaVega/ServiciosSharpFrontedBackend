// Controllers/AdministradoresController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioClientes.API.Data;

namespace ServicioClientes.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // => /api/administradores
    public class AdministradoresController : ControllerBase
    {
        private readonly ClientesDbContext _ctx;
        public AdministradoresController(ClientesDbContext ctx) => _ctx = ctx;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var admins = await _ctx.Administrador
                .Select(a => new {
                    IdAdministrador = a.ID_Administrador,
                    a.Nombre,
                    a.Apellido,
                    NombreCompleto = (a.Nombre + " " + a.Apellido).Trim()
                })
                .OrderBy(a => a.Nombre)
                .ToListAsync();

            return Ok(admins);
        }
    }
}
