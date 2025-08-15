using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class ClienteDto
    {
        public int IdCliente { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? NroDocumento { get; set; }
        public string? Direccion { get; set; }
        public string? NumeroTelf { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string? Correo { get; set; }
        public string? Clave { get; set; }
        public string? Estado { get; set; }


        //Para el filtro
        public int TotalReservas { get; set; }
    }
}
