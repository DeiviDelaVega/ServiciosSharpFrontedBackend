namespace ServicioClientes.API.Models
{
    public class Usuario
    {
        public string Correo { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;

        // Relación inversa
        public Cliente? Cliente { get; set; }
        public Administrador? Administrador { get; set; }
    }
}
