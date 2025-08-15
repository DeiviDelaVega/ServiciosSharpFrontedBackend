using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Shared.Models;
using System.Globalization;
using Frontend.WebApp.Models;

namespace Frontend.WebApp.Controllers
{
    public class InmuebleController : Controller
    {
        private readonly IHttpClientFactory _factory;

        public InmuebleController(IHttpClientFactory factory) => _factory = factory;

        private HttpClient Api()
        {
            var cli = _factory.CreateClient("ServicioInmuebles");
            var token = HttpContext.Session.GetString("token");

            if (!string.IsNullOrEmpty(token))
                cli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return cli;
        }

        // LISTA
        public async Task<IActionResult> Index()
        {
            var res = await Api().GetAsync("api/inmuebles");
            if (!res.IsSuccessStatusCode)
            {
                TempData["Msg"] = "No se pudo listar inmuebles.";
                return View(new List<InmuebleDto>());
            }

            var data = await res.Content.ReadFromJsonAsync<List<InmuebleDto>>();
            return View(data ?? new List<InmuebleDto>());
        }

        // CREAR (GET)
        public IActionResult Create() => View(new CrearInmuebleViewModel());

        // CREAR (POST) - multipart/form-data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearInmuebleViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            using var form = new MultipartFormDataContent();

            form.Add(new StringContent(vm.Nombre ?? ""), "Nombre");
            form.Add(new StringContent(vm.Capacidad.ToString()), "Capacidad");
            form.Add(new StringContent(vm.Numero_Habitaciones.ToString()), "Numero_Habitaciones");
            form.Add(new StringContent(vm.Descripcion ?? ""), "Descripcion");
            form.Add(new StringContent(vm.Servicios_Incluidos ?? ""), "Servicios_Incluidos");
            form.Add(new StringContent(vm.Disponibilidad ?? "Si"), "Disponibilidad");

            // Precio: punto decimal garantizado
            form.Add(new StringContent(
                vm.Precio_Por_Noche.ToString(CultureInfo.InvariantCulture)
            ), "Precio_Por_Noche");

            // Lat/Long: envía TEXTO, normalizado a punto
            var lat = (vm.Latitud ?? "").Replace(',', '.');
            var lng = (vm.Longitud ?? "").Replace(',', '.');
            form.Add(new StringContent(lat), "Latitud");
            form.Add(new StringContent(lng), "Longitud");

            if (vm.Imagen is not null && vm.Imagen.Length > 0)
            {
                using var ms = new MemoryStream();
                await vm.Imagen.CopyToAsync(ms);
                ms.Position = 0;
                var fileContent = new ByteArrayContent(ms.ToArray());
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(vm.Imagen.ContentType);
                form.Add(fileContent, "imagen", vm.Imagen.FileName);
            }

            var res = await Api().PostAsync("api/inmuebles", form);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                ViewBag.Mensaje = $"HTTP {(int)res.StatusCode} ({res.StatusCode}). {body}";
                return View(vm);
            }

            TempData["Msg"] = "Inmueble creado correctamente.";
            return RedirectToAction(nameof(Index));
        }



        // EDITAR (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var res = await Api().GetAsync($"api/inmuebles/{id}");
            if (!res.IsSuccessStatusCode)
            {
                TempData["Msg"] = "No se pudo cargar el inmueble.";
                return RedirectToAction(nameof(Index));
            }

            var model = await res.Content.ReadFromJsonAsync<InmuebleDto>();
            return View(model);
        }

        // EDITAR (POST) – JSON (no cambia imagen)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InmuebleDto dto)
        {
            var res = await Api().PutAsJsonAsync($"api/inmuebles/{dto.IdInmueble}", dto);
            if (!res.IsSuccessStatusCode)
            {
                ViewBag.Mensaje = await res.Content.ReadAsStringAsync();
                return View(dto);
            }

            TempData["Msg"] = "Inmueble actualizado.";
            return RedirectToAction(nameof(Index));
        }

        // ELIMINAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await Api().DeleteAsync($"api/inmuebles/{id}");
            TempData["Msg"] = res.IsSuccessStatusCode ? "Inmueble eliminado." : "No se pudo eliminar.";
            return RedirectToAction(nameof(Index));
        }
    }
}
