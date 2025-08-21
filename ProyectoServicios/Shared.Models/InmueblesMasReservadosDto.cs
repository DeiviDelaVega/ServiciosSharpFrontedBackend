using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class InmueblesMasReservadosDto
    {
        public int ID_Inmueble { get; set; }        // El ID del inmueble
        public int CantidadReservas { get; set; }   // Cuántas reservas tiene
    }
}
