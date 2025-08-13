using System.Text.Json;
using Frontend.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Frontend.WebApp.Controllers
{
    public class ClienteController : Controller
    {
        private readonly HttpClient _httpClient;

        public ClienteController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ServicioClientes"); // Configura esto en Program.cs
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "-1";

            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
            {
                context.Result = RedirectToAction("Login", "Auth");
            }
        }

        //Public
        public IActionResult Index()
        {
            return View();
        }

        //Mantenimiento Cliente

        public async Task<IActionResult> ListClientes(int pageNumber = 1, int pageSize = 5, string filtro = null)
        {
            ViewData["Filtro"] = filtro;
            HttpResponseMessage response;

            if (string.IsNullOrEmpty(filtro))
            {
                response = await _httpClient.GetAsync($"api/admin/cliente/paginacion?pageNumber={pageNumber}&pageSize={pageSize}");
            } 
            else
            {
                var filtroEncoded = Uri.EscapeDataString(filtro);
                response = await _httpClient.GetAsync($"api/admin/cliente/filtro?filtro={filtro}&pageNumber={pageNumber}&pageSize={pageSize}");
            }

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var paginacion = await response.Content.ReadFromJsonAsync<PaginacionResponse>(options);
                return View(paginacion ?? new PaginacionResponse());
            }

            return View(new PaginacionResponse()); // Vacío en caso de error
        }

        public async Task<IActionResult> Details(int id)
        {
            var cliente = await _httpClient.GetFromJsonAsync<Cliente>($"api/admin/cliente/{id}");
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cliente = await _httpClient.GetFromJsonAsync<Cliente>($"api/admin/cliente/{id}");
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Cliente cliente)
        {
            if (id != cliente.ID_Cliente) 
                return BadRequest();

            var response = await _httpClient.PutAsJsonAsync($"api/admin/cliente/{id}", cliente);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Cliente actualizado exitosamente";
                return RedirectToAction("ListClientes");
            }

            ModelState.AddModelError("", "Error al actualizar el cliente");
            return View(cliente);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/admin/cliente/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Cliente eliminado exitosamente";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"No se pudo eliminar el cliente: {error}";
            }
            return RedirectToAction("ListClientes");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/admin/cliente/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Cliente eliminado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, error);
                var cliente = await _httpClient.GetFromJsonAsync<Cliente>($"api/admin/cliente/{id}");
                return View(cliente);
            }

        }
    }
}
