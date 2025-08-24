using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class ReservaDto
    {
        public int ID_Solicitud { get; set; }

        public DateTime Fecha_Solicitud { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El cliente es requerido")]
        public int ID_Cliente { get; set; }

        [Required(ErrorMessage = "El inmueble es requerido")]
        public int ID_Inmueble { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime Fecha_Inicio_Reserva { get; set; }

        [Required(ErrorMessage = "La fecha fin es requerida")]
        public DateTime Fecha_Fin_Reserva { get; set; }

        [Required(ErrorMessage = "El método de pago es requerido")]
        [StringLength(20)]
        [RegularExpression("Tarjeta", ErrorMessage = "Solo se permite el método de pago 'Tarjeta'")]
        public string Metodo_Pago { get; set; } = "Tarjeta";

        [Required(ErrorMessage = "El monto total es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto_Total { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [StringLength(20)]
        [RegularExpression("Solicitado|Aprobado|Cancelado|Finalizado",
            ErrorMessage = "Estado inválido. Valores permitidos: Solicitado, Aprobado, Cancelado, Finalizado")]
        public string Estado_Reserva { get; set; } = "Solicitado";

        public bool Reembolsable { get; set; }
        public long? TiempoRestanteReembolsoMs { get; set; }


        // Propiedades de navegación para mostrar información relacionada
        public ClienteDto? Cliente { get; set; }
        public InmuebleDto? Inmueble { get; set; }
    }
}
