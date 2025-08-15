using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class CrearInmuebleDto
    {
        public string Nombre { get; set; } = "";
        public int Capacidad { get; set; }
        public int Numero_Habitaciones { get; set; }
        public string? Descripcion { get; set; }
        public string? Servicios_Incluidos { get; set; }
        public string? Disponibilidad { get; set; } // "Si" | "No"
        public decimal Precio_Por_Noche { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        // imagen llega como archivo mediante multipart
    }
}
