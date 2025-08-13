using System.Text.Json;

namespace ServicioClientes.API.Data
{
    public static class HttpClientExtensions
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<T?> GetFromJsonAsyncCaseInsensitive<T>(this HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return default;

            return await response.Content.ReadFromJsonAsync<T>(_options);
        }
    }
}
