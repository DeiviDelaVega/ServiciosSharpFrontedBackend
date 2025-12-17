using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido")]
        [StringLength(50, ErrorMessage = "El correo no puede superar los 50 caracteres")]
        public string? Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string? Clave { get; set; }

        public string? DNTCaptchaText { get; set; }
    }
}
