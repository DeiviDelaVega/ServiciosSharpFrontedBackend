using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Frontend.WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shared.Models;

namespace Frontend.WebApp.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CatalogoController> _logger;
        private readonly string _apiBaseUrl;

        public CatalogoController(IHttpClientFactory httpClientFactory, ILogger<CatalogoController> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _apiBaseUrl = configuration["ApiUrls:ServicioInmuebles"];
        }

        // Crea un HttpClient configurado con el token de sesión
        private HttpClient CrearClienteConToken()
        {
            var token = HttpContext.Session.GetString("token");

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("No hay token en sesión.");
                return null;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                if (jwt.ValidTo < DateTime.UtcNow)
                {
                    _logger.LogWarning("Token expirado. Expiró en: {Expiracion}", jwt.ValidTo);
                    return null;
                }

                var rol = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                _logger.LogInformation("Token válido. Rol del usuario: {Rol}", rol ?? "no definido");

                var client = _httpClientFactory.CreateClient("ServicioClientes");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return client;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al analizar el token JWT.");
                return null;
            }
        }



        // Mostrar página principal del cliente (nombre, estado, etc.)
        public async Task<IActionResult> Index()
        {
                var client = CrearClienteConToken();
                if (client == null)
                    return RedirectToAction("Login", "Auth");

                string url = $"{_apiBaseUrl}api/cliente/catalogo/VerInmueble";

                _logger.LogInformation("Llamando a la URL: {Url}", url);

                var response = await client.GetFromJsonAsync<CatalogoResponse>(url);

                if (response == null)
                    _logger.LogWarning("La respuesta es null");

                return View(response);
            }


        // Ver listado de inmuebles con filtros
        public async Task<IActionResult> ListarInmuebles(string filtro = "", decimal? precioDesde = null, decimal? precioHasta = null, string estado = "", int pageNumber = 1)
        {
            try
            {
                ViewData["EstadoOptions"] = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Seleccionar --" },
            new SelectListItem { Value = "Si", Text = "Disponible" },
            new SelectListItem { Value = "No", Text = "Ocupado" }
        };

                var client = CrearClienteConToken();
                if (client == null)
                    return RedirectToAction("Login", "Auth");

                // Ruta relativa sin prefijo redundante (ya definido en BaseAddress)
                string url = $"{_apiBaseUrl}api/cliente/catalogo/ListarInmuebles?pageNumber=1&pageSize=5";

                if (precioDesde.HasValue)
                    url += $"&precioDesde={precioDesde.Value}";

                if (precioHasta.HasValue)
                    url += $"&precioHasta={precioHasta.Value}";

                _logger.LogInformation("Llamando a la URL: {Url}", url);

                var response = await client.GetFromJsonAsync<CatalogoResponse>(url);

                ViewBag.Filtro = filtro;
                ViewBag.PrecioDesde = precioDesde;
                ViewBag.PrecioHasta = precioHasta;
                ViewBag.Estado = estado;

                return View(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inmuebles.");
                ViewBag.ErrorMensaje = ex.ToString();

                var errorModel = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
                return View("Error", errorModel);
            }
        }

        // Ver detalle de un inmueble específico
        public async Task<IActionResult> Detalle(int id)
        {
            try
            {
                var client = CrearClienteConToken();
                if (client == null)
                    return RedirectToAction("Login", "Auth");

                var inmueble = await client.GetFromJsonAsync<InmuebleDto>($"{_apiBaseUrl}api/cliente/catalogo/detalle/{id}");
                if (inmueble == null) return NotFound();
                return View(inmueble);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle del inmueble.");
                return View("Error");
            }
        }

        // Obtener fechas ocupadas de un inmueble
        public async Task<IActionResult> FechasOcupadas(int id)
        {
            try
            {
                var client = CrearClienteConToken();
                if (client == null)
                    return Unauthorized();

                var fechas = await client.GetFromJsonAsync<List<CatalogoResponse>>($"{_apiBaseUrl}api/cliente/catalogo/ocupadas/{id}");
                return Json(fechas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener fechas ocupadas.");
                return StatusCode(500, "Error al cargar las fechas ocupadas");
            }
        }

        // Mostrar términos y condiciones
        public async Task<IActionResult> Terminos()
        {
            try
            {
                var client = CrearClienteConToken();
                if (client == null)
                    return Unauthorized();

                var terminos = await client.GetStringAsync($"{_apiBaseUrl}api/cliente/catalogo/terminos");
                return View("Terminos", terminos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar términos.");
                return View("Error");
            }
        }

        // Mostrar motivo de sanción
        public async Task<IActionResult> MotivoSancion()
        {
            try
            {
                var client = CrearClienteConToken();
                if (client == null)
                    return Unauthorized();

                var motivo = await client.GetStringAsync($"{_apiBaseUrl}api/cliente/catalogo/motivoSancion");
                return View("MotivoSancion", motivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar motivo de sanción.");
                return View("Error");
            }
        }
    }
}
