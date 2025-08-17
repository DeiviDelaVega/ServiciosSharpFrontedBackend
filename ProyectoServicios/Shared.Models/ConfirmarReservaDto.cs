using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class ConfirmarReservaDto
    {
        public int IdCliente { get; set; }
        public int IdInmueble { get; set; }
        public DateTime FechaInicio { get; set; }  // yyyy-MM-dd
        public DateTime FechaFin { get; set; }     // yyyy-MM-dd
        public decimal MontoTotal { get; set; }
        public string StripePaymentId { get; set; }
    }
}
