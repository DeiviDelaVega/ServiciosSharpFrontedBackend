using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Frontend.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shared.Models;

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
        private HttpClient ApiClientes()
        {
            var cli = _factory.CreateClient("ServicioClientes");
            var token = HttpContext.Session.GetString("token");
            if (!string.IsNullOrEmpty(token))
                cli.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            return cli;
        }

        // Frontend.WebApp.Controllers.InmuebleController

        public async Task<IActionResult> Index(string filtro, string disponibilidad, int? adminId, int pagina = 1, int pageSize = 5)
        {
            // 1) Traer inmuebles
            var res = await Api().GetAsync("api/inmuebles");
            if (!res.IsSuccessStatusCode)
            {
                TempData["Msg"] = "No se pudo listar inmuebles.";
                return View(new List<InmuebleDto>());
            }
            var data = await res.Content.ReadFromJsonAsync<List<InmuebleDto>>() ?? new List<InmuebleDto>();

            // 2) Filtros
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                data = data.Where(i =>
                    (i.Nombre?.Contains(filtro, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (i.Descripcion?.Contains(filtro, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (i.ServiciosIncluidos?.Contains(filtro, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(disponibilidad))
            {
                data = data.Where(i => string.Equals(i.Disponibilidad, disponibilidad, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (adminId.HasValue)
            {
                // filtra por el creador
                data = data.Where(i => i.ID_Admin_Creador == adminId.Value).ToList();
            }

            // 3) Orden + paginación
            data = data.OrderBy(i => i.IdInmueble).ToList();
            var totalItems = data.Count;
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            pagina = Math.Clamp(pagina, 1, totalPaginas);
            var paginaActual = data.Skip((pagina - 1) * pageSize).Take(pageSize).ToList();

            // 4) Poblar combo de administradores (desde ServicioClientes.API)
            var resAdmins = await ApiClientes().GetAsync("api/administradores");
            if (resAdmins.IsSuccessStatusCode)
            {
                var admins = await resAdmins.Content.ReadFromJsonAsync<List<AdministradorDto>>() ?? new();
                var adminsSelect = admins.Select(a => new {
                    a.IdAdministrador,
                    NombreCompleto = $"{a.Nombre} {a.Apellido}".Trim()
                }).ToList();

                ViewBag.Administradores = new SelectList(adminsSelect, "IdAdministrador", "NombreCompleto", adminId);
            }
            else
            {
                ViewBag.Administradores = new SelectList(Enumerable.Empty<object>(), "IdAdministrador", "NombreCompleto");
            }

            // 5) Variables para la vista
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.Filtro = filtro;
            ViewBag.Disponibilidad = disponibilidad;
            ViewBag.AdminId = adminId;

            return View(paginaActual);
        }



        // CREAR (GET)
        public IActionResult Create() => View(new CrearInmuebleViewModel());

        // CREAR (POST) - multipart/form-data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearInmuebleViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Por favor complete todos los campos correctamente." });

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(vm.Nombre ?? ""), "Nombre");
            form.Add(new StringContent(vm.Capacidad.ToString()), "Capacidad");
            form.Add(new StringContent(vm.Numero_Habitaciones.ToString()), "Numero_Habitaciones");
            form.Add(new StringContent(vm.Descripcion ?? ""), "Descripcion");
            form.Add(new StringContent(vm.Servicios_Incluidos ?? ""), "Servicios_Incluidos");
            form.Add(new StringContent(vm.Disponibilidad ?? "Si"), "Disponibilidad");
            form.Add(new StringContent(vm.Precio_Por_Noche.ToString(CultureInfo.InvariantCulture)), "Precio_Por_Noche");

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
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.Imagen.ContentType);
                form.Add(fileContent, "imagen", vm.Imagen.FileName);
            }

            var res = await Api().PostAsync("api/inmuebles", form);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                return Json(new { success = false, message = $"HTTP {(int)res.StatusCode}: {body}" });
            }

            // ✅ Devuelve JSON indicando éxito
            return Json(new { success = true, message = "Inmueble creado correctamente." });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InmuebleDto dto, IFormFile imagen)
        {
            using var form = new MultipartFormDataContent();

            form.Add(new StringContent(dto.Nombre ?? ""), "Nombre");
            form.Add(new StringContent(dto.Capacidad.ToString()), "Capacidad");
            form.Add(new StringContent(dto.NumeroHabitaciones.ToString()), "Numero_Habitaciones");
            form.Add(new StringContent(dto.Descripcion ?? ""), "Descripcion");
            form.Add(new StringContent(dto.ServiciosIncluidos ?? ""), "Servicios_Incluidos");
            form.Add(new StringContent(dto.Disponibilidad ?? "Si"), "Disponibilidad");
            form.Add(new StringContent(dto.PrecioPorNoche.ToString(CultureInfo.InvariantCulture)), "Precio_Por_Noche");

            // 👇 toma los valores crudos del form (no los del dto) y normaliza
            var latStr = (Request.Form["Latitud"].ToString() ?? "").Replace(',', '.');
            var lngStr = (Request.Form["Longitud"].ToString() ?? "").Replace(',', '.');
            form.Add(new StringContent(latStr), "Latitud");
            form.Add(new StringContent(lngStr), "Longitud");

            if (imagen != null && imagen.Length > 0)
            {
                using var ms = new MemoryStream();
                await imagen.CopyToAsync(ms);
                ms.Position = 0;
                var fileContent = new ByteArrayContent(ms.ToArray());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(imagen.ContentType);
                form.Add(fileContent, "imagen", imagen.FileName);
            }

            var res = await Api().PutAsync($"api/inmuebles/{dto.IdInmueble}", form);
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

        public async Task<IActionResult> Details(int id)
        {
            var res = await Api().GetAsync($"api/inmuebles/{id}");
            if (!res.IsSuccessStatusCode)
            {
                TempData["Msg"] = "No se pudo cargar el inmueble.";
                return RedirectToAction(nameof(Index));
            }

            var inmueble = await res.Content.ReadFromJsonAsync<InmuebleDto>();
            return View(inmueble);
        }

    }
}
