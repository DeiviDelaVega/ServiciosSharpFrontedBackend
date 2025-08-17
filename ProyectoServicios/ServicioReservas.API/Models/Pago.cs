namespace ServicioReservas.API.Models
{
    public class Pago
    {
        public int ID_Pago { get; set; }
        public int ID_Solicitud { get; set; }
        public DateTime Fecha_Pago { get; set; } = DateTime.Now;
        public decimal Monto { get; set; }
        public string Stripe_Payment_Id { get; set; }
    }
}
