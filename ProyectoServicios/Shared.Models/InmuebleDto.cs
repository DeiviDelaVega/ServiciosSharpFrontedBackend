using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class InmuebleDto
    {
        public int IdInmueble { get; set; }
        public string? Nombre { get; set; }
        public int Capacidad { get; set; }
        public int NumeroHabitaciones { get; set; }
        public string? Descripcion { get; set; }
        public string? ServiciosIncluidos { get; set; }
        public string? Disponibilidad { get; set; }
        public decimal PrecioPorNoche { get; set; }
        public string? ImagenHabitacion { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
    }
}
