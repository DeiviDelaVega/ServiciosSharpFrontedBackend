using Shared.Models;
using System.Net.Http.Json;

namespace ServicioInmuebles.API.Service
{
    public class ClienteService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(HttpClient httpClient, ILogger<ClienteService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ClienteDto?> GetClientePorCorreoAsync(string correo)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ClienteDto>($"api/admin/cliente/correo/{correo}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error al llamar a ServicioClientes.API para obtener cliente por correo: {Correo}", correo);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en GetClientePorCorreoAsync.");
                return null;
            }
        }

        public async Task<List<ClienteDto>> GetClientesAsync()
        {
            try
            {
                var clientes = await _httpClient.GetFromJsonAsync<List<ClienteDto>>("api/admin/cliente");
                return clientes ?? new List<ClienteDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error al llamar a ServicioClientes.API para obtener lista de clientes.");
                return new List<ClienteDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en GetClientesAsync.");
                return new List<ClienteDto>();
            }
        }
    }
}
