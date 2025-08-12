using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class AdministradorDto
    {
        public int IdAdministrador { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? NroDocumento { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Clave { get; set; }
    }
}
