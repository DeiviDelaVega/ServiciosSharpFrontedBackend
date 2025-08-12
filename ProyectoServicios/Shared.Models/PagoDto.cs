using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    internal class PagoDto
    {
        public int IdPago { get; set; }
        public int IdSolicitud { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Monto { get; set; }
        public string? StripePaymentId { get; set; }
    }
}
