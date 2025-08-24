using Shared.Models;

namespace Frontend.WebApp.Models
{
    public class MisReservasViewModel
    {
        public List<ReservaDto> Reservas { get; set; } = new();
        public bool EsActivo { get; set; }
        public bool ModalSancion { get; set; }
        public string? Alerta { get; set; }
        public string? NombreCliente { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalElementos { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }
}
