using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class ClienteDto
    {
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El apellido debe tener entre 3 y 50 caracteres")]
        public string? Apellido { get; set; }

        [Required(ErrorMessage = "El Nro de Documento es obligatorio")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "El número de documento debe tener entre 8 y 50 caracteres")]
        public string? NroDocumento { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "La dirección debe tener entre 5 y 200 caracteres")]
        public string? Direccion { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(50, MinimumLength = 9, ErrorMessage = "El teléfono debe tener entre 9 y 50 caracteres")]
        public string? NumeroTelf { get; set; }
        
        public DateTime? FechaRegistro { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "El correo debe tener entre 5 y 50 caracteres")]
        public string? Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 255 caracteres")]
        public string? Clave { get; set; }
        
        public string? Estado { get; set; }

        // Para el filtro
        public int TotalReservas { get; set; }
    }
}
