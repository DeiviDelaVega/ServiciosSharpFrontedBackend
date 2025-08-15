using System;
using System.ComponentModel.DataAnnotations;

namespace ServicioReservas.API.Models
{
    public class Cliente
    {
        [Key]
        public int ID_Cliente { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NroDocumento { get; set; }
        public string Direccion { get; set; }
        public string NumeroTelf { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Estado { get; set; }
        public string Correo { get; set; }
    }
}