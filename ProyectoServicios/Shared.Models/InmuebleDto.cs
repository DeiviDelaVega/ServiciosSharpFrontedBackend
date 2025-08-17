using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class InmuebleDto
    {
        [Column("ID_Inmueble")]
        [JsonPropertyName("iD_Inmueble")]
        public int IdInmueble { get; set; }

        [Column("Nombre")]
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [Column("Capacidad")]
        [JsonPropertyName("capacidad")]
        public int Capacidad { get; set; }

        [Column("Numero_Habitaciones")]
        [JsonPropertyName("numero_Habitaciones")]
        public int NumeroHabitaciones { get; set; }

        [Column("Descripcion")]
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [Column("Servicios_Incluidos")]
        [JsonPropertyName("servicios_Incluidos")]
        public string ServiciosIncluidos { get; set; }

        [Column("Disponibilidad")]
        [JsonPropertyName("disponibilidad")]
        public string Disponibilidad { get; set; }

        [Column("Precio_Por_Noche")]
        [JsonPropertyName("precio_Por_Noche")]
        public decimal PrecioPorNoche { get; set; }


        [Column("Imagen_Habitacion")]
        [JsonPropertyName("imagen_Habitacion")]
        public string ImagenHabitacion { get; set; }

        [Column("Latitud")]
        [JsonPropertyName("latitud")]
        public decimal? Latitud { get; set; }

        [Column("Longitud")]
        [JsonPropertyName("longitud")]
        public decimal? Longitud { get; set; }

       
        [JsonPropertyName("iD_Admin_Creador")]
        public int? ID_Admin_Creador { get; set; }
    }
}
