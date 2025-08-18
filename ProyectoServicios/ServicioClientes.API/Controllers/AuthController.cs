using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ServicioClientes.API.Data;
using ServicioClientes.API.Models;
using Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ServicioClientes.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {

        private readonly ClientesDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ClientesDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register-cliente")]
        public IActionResult RegisterCliente(ClienteDto dto)
        {

            if (_context.Usuario.Any(u => u.Correo == dto.Correo))
            {
                return BadRequest("El correo ya esta registrado.");
            }

            var usuario = new Usuario
            {
                Correo = dto.Correo!,
                Clave = BCrypt.Net.BCrypt.HashPassword(dto.Clave),
                Rol = "cliente"
            };

            var cliente = new Cliente
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                NroDocumento = dto.NroDocumento,
                Direccion = dto.Direccion,
                NumeroTelf = dto.NumeroTelf,
                Estado = "activo",
                Correo = dto.Correo,
                FechaRegistro = DateTime.Now
            };

            _context.Usuario.Add(usuario);
            _context.Cliente.Add(cliente);
            _context.SaveChanges();

            return Ok("Cliente registrado exitosamente");
        }

        [HttpPost("register-admin")] 
        public IActionResult RegisterAdmin(AdministradorDto dto)
        {

            if (_context.Usuario.Any(u => u.Correo == dto.Correo))
            {
                return BadRequest("El correo ya esta registrado.");
            }

            var usuario = new Usuario
            {
                Correo = dto.Correo!,
                Clave = BCrypt.Net.BCrypt.HashPassword(dto.Clave),
                Rol = "admin"
            };

            var admin = new Administrador
            {
                Nombre = dto.Nombre!,
                Apellido = dto.Apellido!,
                NroDocumento = dto.NroDocumento!,
                Telefono = dto.Telefono!,
                Correo = dto.Correo!

            };

            _context.Usuario.Add(usuario);
            _context.Administrador.Add(admin);
            _context.SaveChanges();

            return Ok("Administrador registrado exitosamente");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            var usuario = _context.Usuario.SingleOrDefault(u => u.Correo == dto.Correo);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Clave, usuario.Clave))
                return Unauthorized("Credenciales invalidas");

            int? adminId = null;
            string nombreCompleto = "";

            if (usuario.Rol == "admin")
            {
                var admin = _context.Administrador.FirstOrDefault(a => a.Correo == usuario.Correo);
                adminId = admin?.ID_Administrador;
                nombreCompleto = $"{admin?.Nombre} {admin?.Apellido}";
            }
            else if (usuario.Rol == "cliente")
            {
                var cliente = _context.Cliente.FirstOrDefault(c => c.Correo == usuario.Correo);
                nombreCompleto = $"{cliente?.Nombre} {cliente?.Apellido}";
            }

            var token = GenerateJwtToken(usuario, adminId, nombreCompleto);

            return Ok(new {token});
        }

        private string GenerateJwtToken(Usuario usuario, int? adminId, string nombreCompleto)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Correo!),
                new Claim(ClaimTypes.Role, usuario.Rol!),
                new Claim("NombreCompleto", nombreCompleto)
            };

            if (adminId.HasValue)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, adminId.Value.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
                
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
