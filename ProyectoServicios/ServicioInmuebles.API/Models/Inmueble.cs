namespace ServicioInmuebles.API.Models
{
    public class Inmueble
    {
        public int ID_Inmueble { get; set; }
        public string Nombre { get; set; } = "";
        public int Capacidad { get; set; }
        public int Numero_Habitaciones { get; set; }
        public string? Descripcion { get; set; }
        public string? Servicios_Incluidos { get; set; }
        public string? Disponibilidad { get; set; }   // "Si" | "No"
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public string? Imagen_Habitacion { get; set; }  // URL Cloudinary
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
    }
}
