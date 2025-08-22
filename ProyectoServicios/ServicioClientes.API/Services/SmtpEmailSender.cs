using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace ServicioClientes.API.Services
{
    public class SmtpEmailSender : IEmailSender
    {

        private readonly IConfiguration _cfg;
        public SmtpEmailSender(IConfiguration cfg) => _cfg = cfg;
        public async Task SendWelcomeAsync(string toEmail, string nombre)
        {
            var host = _cfg["Smtp:Host"];
            var port = int.Parse(_cfg["Smtp:Port"]!);
            var ssl = bool.Parse(_cfg["Smtp:EnableSsl"]!);
            var user = _cfg["Smtp:User"];
            var pass = _cfg["Smtp:Pass"];
            var from = _cfg["Smtp:From"];

            using var smtp = new SmtpClient(host, port)
            {
                EnableSsl = ssl,
                Credentials = new NetworkCredential(user, pass)
            };

            var body = $@"
            <div style='font-family:Arial,sans-serif'>
              <h2>¡Bienvenido, {WebUtility.HtmlEncode(nombre)}!</h2>
              <p>Tu registro en <b>Monterrico Polo Aparts</b> fue exitoso.</p>
              <p>Ya puedes iniciar sesión y reservar tu hospedaje.</p>
              <hr/>
              <small>Este es un mensaje automático, no responder.</small>
            </div>";

            using var msg = new MailMessage(from!, toEmail)
            {
                Subject = "¡Bienvenido a Monterrico Polo Aparts!",
                Body = body,
                IsBodyHtml = true
            };

            await smtp.SendMailAsync(msg);
        }
    }
}
