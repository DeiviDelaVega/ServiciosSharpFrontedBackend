using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class CatalogoResponse
    {
        public string NombreCliente { get; set; }
        public List<InmuebleDto> Inmuebles { get; set; } = new List<InmuebleDto>();
        public int TotalPaginas { get; set; }
        public int PaginaActual { get; set; }
        public bool ModalSancion { get; set; }
        public string Alerta { get; set; }
        public string Motivo { get; set; }
        public DateTime Fecha_Inicio_Reserva { get; set; }
        public DateTime Fecha_Fin_Reserva { get; set; }
        public string Estado { get; set; }

    }
}