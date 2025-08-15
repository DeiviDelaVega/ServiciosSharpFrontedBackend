using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioReservas.API.Models
{
    public class Reserva
    {
        [Key]
        public int ID_Solicitud { get; set; }

        public DateTime Fecha_Solicitud { get; set; }

        public int ID_Cliente { get; set; }
        public int ID_Inmueble { get; set; }

        public DateTime Fecha_Inicio_Reserva { get; set; }
        public DateTime Fecha_Fin_Reserva { get; set; }
        public string Metodo_Pago { get; set; }
        public decimal Monto_Total { get; set; }
        public string Estado_Reserva { get; set; }

        // Elimina las propiedades de navegación
        // public Cliente Cliente { get; set; }
        // public Inmueble Inmueble { get; set; }
    }
}