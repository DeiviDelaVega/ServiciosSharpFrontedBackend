using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioClientes.API.Models
{
    public class Administrador
    {
        [Key] public int ID_Administrador { get; set; } // Mapea con ID_Administrador
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        [Column("Nro_Documento")]
        public string NroDocumento { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;

        // Clave foránea
        public string Correo { get; set; } = string.Empty;

        // Relación con Usuario
        public Usuario Usuario { get; set; } = null!;
    }

}
