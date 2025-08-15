using Shared.Models;

namespace Frontend.WebApp.Models
{
    public class ReservaIndexViewModel
    {
        public List<ReservaDto> reservas { get; set; } = new();
        public int totalPages { get; set; }
        public int currentPage { get; set; }
        public int totalItems { get; set; }
    }
}
