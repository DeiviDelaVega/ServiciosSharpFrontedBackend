using ServicioClientes.API.Models;

namespace ServicioClientes.API.Controllers
{
    internal class PaginacionResponse
    {
        public int TotalRecords { get; set; }
        public List<Cliente> Items { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}