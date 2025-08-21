using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shared.Models;

namespace Frontend.WebApp.Controllers
{
    public class ReportesController : Controller
    {
        private readonly HttpClient _httpClient;

        public ReportesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IActionResult> InmueblesMasReservados()
        {
            var response = await _httpClient.GetStringAsync("https://localhost:7185/api/Reportes/InmueblesMasReservados");
            var reservas = JsonConvert.DeserializeObject<List<InmueblesMasReservadosDto>>(response);
            var listaFinal = new List<(string Nombre, int Cantidad)>();

            foreach (var r in reservas)
            {
                try
                {
                    // Llamada a ServicioInmuebles.API
                    var respInm = await _httpClient.GetStringAsync($"https://localhost:7014/api/Inmuebles/Obtener/{r.ID_Inmueble}");
                    var inmueble = JsonConvert.DeserializeObject<InmuebleDto>(respInm);

                    listaFinal.Add((inmueble.Nombre, r.CantidadReservas));
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"No se encontró inmueble con ID {r.ID_Inmueble}: {ex.Message}");
                    listaFinal.Add(($"Inmueble {r.ID_Inmueble} (no encontrado)", r.CantidadReservas));
                }
            }
            ViewBag.Datos = listaFinal;
            return View();
        }

        public async Task<IActionResult> ClientesConMasReservas()
        {
            var response = await _httpClient.GetStringAsync("https://localhost:7185/api/Reportes/ClientesConMasReservas");
            var clientes = JsonConvert.DeserializeObject<List<ClientesConMasReservasDto>>(response);
            var listaFinal = new List<(string NombreCompleto, int Cantidad)>();

            foreach (var c in clientes)
            {
                try
                {
                    var respCliente = await _httpClient.GetStringAsync($"https://localhost:7100/api/admin/Cliente/Obtener/{c.ID_Cliente}");
                    var clienteDto = JsonConvert.DeserializeObject<ClienteDto>(respCliente);

                    listaFinal.Add(($"{clienteDto.Nombre} {clienteDto.Apellido}", c.CantidadReservas));
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"No se encontra el cliente con ID {c.ID_Cliente}: {ex.Message}");
                    listaFinal.Add(($"Cliente {c.ID_Cliente} (no encontrado)", c.CantidadReservas));
                }
            }
            ViewBag.DatosClientes = listaFinal;
            return View();
        }
    }
}
