namespace ServicioClientes.API.Services
{
    public interface IEmailSender
    {
        Task SendWelcomeAsync(string toEmail, string nombre);
    }
}
