using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Frontend.WebApp.Models;    // <- ViewModels (abajo te doy uno base)
using Shared.Models;            // <- tus DTOs
using System.Text;

namespace Frontend.WebApp.Controllers
{
    public class MisReservasController : Controller
    {
        private readonly HttpClient _http;

        public MisReservasController(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ServicioReservas");
        }

        // GET: /MisReservas?page=0&size=5
        public async Task<IActionResult> Index(int page = 1, int size = 5)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var req = new HttpRequestMessage(HttpMethod.Get, $"api/cliente/misreservas?page={page}&size={size}");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
            {
                ViewBag.Error = "No se pudieron obtener las reservas.";
                return View(new MisReservasViewModel());
            }

            var jsonStr = await resp.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(jsonStr).RootElement;

            var vm = new MisReservasViewModel
            {
                EsActivo = root.GetProperty("esActivo").GetBoolean(),
                ModalSancion = root.GetProperty("modalSancion").GetBoolean(),
                Alerta = root.TryGetProperty("alerta", out var al) ? al.GetString() : null,
                NombreCliente = root.TryGetProperty("nombreCliente", out var nom) ? nom.GetString() : null,
                Reservas = JsonSerializer.Deserialize<List<ReservaDto>>(root.GetProperty("reservas").GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new(),
                Page = root.GetProperty("page").GetProperty("paginaActual").GetInt32(),
                Size = root.GetProperty("page").GetProperty("size").GetInt32(),
                TotalPaginas = root.GetProperty("page").GetProperty("totalPaginas").GetInt32(),
                TotalElementos = root.GetProperty("page").GetProperty("totalElementos").GetInt32(),
                HasNext = root.GetProperty("page").GetProperty("hasNext").GetBoolean(),
                HasPrevious = root.GetProperty("page").GetProperty("hasPrevious").GetBoolean()
            };
            ViewBag.PaginaActual = vm.Page;
            ViewBag.TotalPaginas = vm.TotalPaginas;

            return View(vm);
        }

        // POST: /MisReservas/Reembolsar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reembolsar(int id, int page = 1, int size = 5)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var req = new HttpRequestMessage(HttpMethod.Delete, $"api/cliente/misreservas/{id}/reembolso");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode)
                TempData["Ok"] = "Reserva reembolsada correctamente.";
            else
            {
                var err = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = string.IsNullOrWhiteSpace(err) ? "No se pudo reembolsar la reserva." : err;
            }

            return RedirectToAction("Index");
        }
    }
}
