using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class InmuebleDto
    {
        [JsonPropertyName("iD_Inmueble")]
        public int IdInmueble { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("capacidad")]
        public int Capacidad { get; set; }

        [JsonPropertyName("numero_Habitaciones")]
        public int NumeroHabitaciones { get; set; }

        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }

        [JsonPropertyName("servicios_Incluidos")]
        public string? ServiciosIncluidos { get; set; }

        [JsonPropertyName("disponibilidad")]
        public string? Disponibilidad { get; set; }

        [JsonPropertyName("precio_Por_Noche")]
        public decimal PrecioPorNoche { get; set; }

        [JsonPropertyName("imagen_Habitacion")]
        public string? ImagenHabitacion { get; set; }

        [JsonPropertyName("latitud")]
        public decimal? Latitud { get; set; }

        [JsonPropertyName("longitud")]
        public decimal? Longitud { get; set; }

        [JsonPropertyName("iD_Admin_Creador")]
        public int? ID_Admin_Creador { get; set; }
    }
}
