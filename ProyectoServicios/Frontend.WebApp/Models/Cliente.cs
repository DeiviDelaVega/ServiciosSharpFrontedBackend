using System.ComponentModel.DataAnnotations;

namespace Frontend.WebApp.Models
{
    public class Cliente
    {
        [Display(Name = "ID")]
        public int ID_Cliente { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        [Display(Name = "NOMBRE")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres")]
        [Display(Name = "APELLIDO")]
        public string? Apellido { get; set; }

        [Required(ErrorMessage = "El número de documento es obligatorio")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El número de documento debe tener 8 dígitos")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El número de documento debe contener solo números")]
        [Display(Name = "N.° DOCUMENTO")]
        public string? NroDocumento { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres")]
        [Display(Name = "DIRECCION")]
        public string? Direccion { get; set; }

        [Required(ErrorMessage = "El número de teléfono es obligatorio")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de teléfono debe tener 9 dígitos")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "El número de teléfono debe contener solo números")]
        [Display(Name = "TELEFONO")]
        public string? NumeroTelf { get; set; }

        [Required(ErrorMessage = "La fecha de registro es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "FECA DE REGISTRO")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido")]
        [StringLength(50, ErrorMessage = "El correo no puede superar los 50 caracteres")]
        [Display(Name = "CORREO")]
        public string? Correo { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [StringLength(20, ErrorMessage = "El estado no puede superar los 20 caracteres")]
        [RegularExpression(@"^(activo|sancionado)$", ErrorMessage = "El estado debe ser 'activo' o 'sancionado'")]
        [Display(Name = "ESTADO")]
        public string? Estado { get; set; } = "activo";
    }
}
