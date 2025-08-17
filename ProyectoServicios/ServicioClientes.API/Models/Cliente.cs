using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioClientes.API.Models
{
    public class Cliente
    {
        public int ID_Cliente { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        [Column("Nro_Documento")]
        public string? NroDocumento { get; set; }
        public string? Direccion { get; set; }
        [Column("Numero_Telf")]
        public string? NumeroTelf { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? FechaRegistro { get; set; }
        public string Estado { get; set; } = "activo";
        public string? Correo { get; set; }
        public Usuario? Usuario { get; set; }
    }

}
