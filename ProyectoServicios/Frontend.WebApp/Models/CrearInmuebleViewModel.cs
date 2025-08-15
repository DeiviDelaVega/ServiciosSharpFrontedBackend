namespace Frontend.WebApp.Models
{
    public class CrearInmuebleViewModel
    {
        public string? Nombre { get; set; }
        public int Capacidad { get; set; }
        public int Numero_Habitaciones { get; set; }
        public string? Descripcion { get; set; }
        public string? Servicios_Incluidos { get; set; }
        public string? Disponibilidad { get; set; } = "Si";
        public decimal Precio_Por_Noche { get; set; }
        public string? Latitud { get; set; }
        public string? Longitud { get; set; }
        public IFormFile? Imagen { get; set; }
    }
}
