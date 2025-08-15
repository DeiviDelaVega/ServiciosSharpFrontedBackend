using Frontend.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using System.Text.Json;

namespace Frontend.WebApp.Controllers
{
    public class ReservaController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiBaseUrl;

        public ReservaController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = _configuration["ServicioReservas:Url"] ?? "http://localhost:5188/api";
        }

        public async Task<IActionResult> Index(int page = 0)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Reserva?page={page}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonSerializer.Deserialize<ReservaIndexViewModel>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return View(resultado);
                }
                TempData["Error"] = "Error al cargar las reservas";
                return View(new ReservaIndexViewModel());
            }
            catch (Exception)
            {
                TempData["Error"] = "Error al comunicarse con el servicio de reservas";
                return View(new ReservaIndexViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> FiltrarPorFechas([FromBody] FiltrarFechasRequest request)
        {
            if (string.IsNullOrEmpty(request.fechaInicio) || string.IsNullOrEmpty(request.fechaFin))
                return Json(new { error = "Fechas inválidas" });

            var response = await _httpClient.GetAsync(
                $"{_apiBaseUrl}/Reserva/filtrar-fechas?fechaInicio={request.fechaInicio}&fechaFin={request.fechaFin}&page={request.page}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<ReservaIndexViewModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return Json(resultado);
            }
            return Json(new { error = "Error al filtrar reservas" });
        }

        [HttpPost]
        public async Task<IActionResult> FiltrarPorEstado([FromBody] FiltrarEstadoRequest request)
        {
            if (string.IsNullOrEmpty(request.estado))
                return Json(new { error = "Estado inválido" });

            var response = await _httpClient.GetAsync(
                $"{_apiBaseUrl}/Reserva/filtrar-estado?estado={request.estado}&page={request.page}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<ReservaIndexViewModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return Json(resultado);
            }
            return Json(new { error = "Error al filtrar reservas" });
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarEstado([FromQuery] int id, [FromQuery] string estado)
        {
            if (id <= 0 || string.IsNullOrEmpty(estado))
                return Json(new { success = false, message = "Datos inválidos" });

            var response = await _httpClient.PutAsync(
                $"{_apiBaseUrl}/Reserva/{id}/estado?estado={estado}", null);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Estado actualizado correctamente" });
            }
            return Json(new { success = false, message = "Error al actualizar estado" });
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar([FromQuery] int id)
        {
            if (id <= 0)
                return Json(new { success = false, message = "ID inválido" });

            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/Reserva/{id}");
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Reserva eliminada correctamente" });
            }
            return Json(new { success = false, message = "Error al eliminar reserva" });
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Reserva/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var reserva = JsonSerializer.Deserialize<ReservaDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return View(reserva);
            }
            TempData["Error"] = "No se encontró la reserva";
            return RedirectToAction("Index");
        }
    }

    public class FiltrarFechasRequest
    {
        public string fechaInicio { get; set; }
        public string fechaFin { get; set; }
        public int page { get; set; }
    }

    public class FiltrarEstadoRequest
    {
        public string estado { get; set; }
        public int page { get; set; }
    }
}