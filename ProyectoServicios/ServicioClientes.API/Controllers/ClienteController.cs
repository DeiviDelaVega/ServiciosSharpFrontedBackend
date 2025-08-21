using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioClientes.API.Data;
using ServicioClientes.API.Models;
using Shared.Models;

namespace ServicioClientes.API.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class ClienteController : ControllerBase
    {
        private readonly ClientesDbContext _context;

        public ClienteController(ClientesDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _context.Cliente.ToListAsync();
            return Ok(clientes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCliente(int id)
        {
            var cliente = await _context.Cliente.FindAsync(id);
            if (cliente == null) return NotFound();
            return Ok(cliente);
        }

        [HttpGet("correo/{correo}")]
        public async Task<IActionResult> GetClientePorCorreo(string correo)
        {
            var cliente = await _context.Cliente.FirstOrDefaultAsync(c => c.Correo == correo);
            if (cliente == null) return NotFound();
            return Ok(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> InsertCliente([FromBody] Cliente cliente)
        {
            if (cliente == null) return BadRequest("Cliente inválido");

            // Validar si correo ya existe
            bool existeCorreo = await _context.Cliente.AnyAsync(c => c.Correo == cliente.Correo);
            if (existeCorreo) return BadRequest("El correo ya está registrado");

            cliente.FechaRegistro = DateTime.Now;
            cliente.Estado = "activo";

            await _context.Cliente.AddAsync(cliente);
            await _context.SaveChangesAsync();

            return Ok("Cliente registrado exitosamente");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCliente(int id, [FromBody] Cliente cliente)
        {
            if (id != cliente.ID_Cliente) return BadRequest("El ID no coincide");

            var clienteExistente = await _context.Cliente.FindAsync(id);
            if (clienteExistente == null) return NotFound("Cliente no encontrado");

            clienteExistente.Nombre = cliente.Nombre;
            clienteExistente.Apellido = cliente.Apellido;
            clienteExistente.NroDocumento = cliente.NroDocumento;
            clienteExistente.Direccion = cliente.Direccion;
            clienteExistente.NumeroTelf = cliente.NumeroTelf;
            clienteExistente.Correo = cliente.Correo;
            clienteExistente.Estado = cliente.Estado;

            _context.Cliente.Update(clienteExistente);
            await _context.SaveChangesAsync();

            return Ok("Cliente actualizado exitosamente");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Cliente.FindAsync(id);
            if (cliente == null) return NotFound("Cliente no encontrado");

            _context.Cliente.Remove(cliente);
            await _context.SaveChangesAsync();

            return Ok("Cliente eliminado exitosamente");
        }

        //Filtros + Paginacion

        [HttpGet("paginacion")]
        public async Task<IActionResult> GetClientesPaginados([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            var query = _context.Cliente.AsQueryable();

            var totalRegistros = await query.CountAsync();
            var clientes = await query
                .OrderBy(c => c.ID_Cliente)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new PaginacionResponse
            {
                TotalRecords = totalRegistros,
                Items = clientes,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };

            return Ok(response);
        }

        [HttpGet("filtro")]
        public async Task<IActionResult> GetClientesConFiltro([FromQuery] string filtro, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            var query = _context.Cliente.AsQueryable();

            if (!string.IsNullOrEmpty(filtro))
            {
                query = query.Where(c =>
                    c.Apellido!.ToLower().Contains(filtro) ||
                    c.NroDocumento!.ToLower().Contains(filtro));
            }

            var totalItems = await query.CountAsync();
            var clientes = await query
                .OrderBy(c => c.ID_Cliente)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { TotalRegistros = totalItems, Items = clientes,
                            PageNumber = pageNumber, PageSize = pageSize,
            });
        }

        [HttpGet("mas-reservas")]
        public async Task<IActionResult> GetClientesMasReservas()
        {
            var resultado = await _context.ClientesMasReservas
                                            .FromSqlRaw("usp_ClientesMasReservas")
                                            .ToListAsync();

            return Ok(resultado);
        }

        [HttpGet("me")]
        [Authorize(Roles = "cliente")] // Solo clientes pueden consultar sus datos
        public async Task<IActionResult> GetDatosClienteActual()
        {
            var correo = User.Identity?.Name; // del token
            if (string.IsNullOrEmpty(correo))
                return Unauthorized("No se pudo obtener el correo del token");

            var cliente = await _context.Cliente
                .Where(c => c.Correo == correo)
                .Select(c => new ClienteDto
                {
                    IdCliente = c.ID_Cliente,
                    Nombre = c.Nombre,
                    Apellido = c.Apellido,
                    NroDocumento = c.NroDocumento,
                    Direccion = c.Direccion,
                    NumeroTelf = c.NumeroTelf,
                    FechaRegistro = c.FechaRegistro,
                    Correo = c.Correo,
                    Estado = c.Estado
                })
                .FirstOrDefaultAsync();

            if (cliente == null)
                return NotFound("Cliente no encontrado");

            return Ok(cliente);
        }

        // Para Reporte
        [HttpGet("Obtener/{id}")]
        public IActionResult GetClientes(int id)
        {
            var cliente = _context.Cliente
                .Where(c => c.ID_Cliente == id)
                .Select(c => new ClienteDto
                {
                    IdCliente = c.ID_Cliente,
                    Nombre = c.Nombre,
                    Apellido = c.Apellido
                })
                .FirstOrDefault();

            if (cliente == null) return NotFound();
            return Ok(cliente);
        }
    }
}
