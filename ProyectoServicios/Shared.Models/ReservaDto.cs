using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class ReservaDto
    {
        public int IdSolicitud { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int IdCliente { get; set; }
        public int IdInmueble { get; set; }
        public DateTime FechaInicioReserva { get; set; }
        public DateTime FechaFinReserva { get; set; }
        public string? MetodoPago { get; set; } 
        public decimal MontoTotal { get; set; }
        public string? EstadoReserva { get; set; } 
    }
}
