using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Net.Mail;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Shared.Models; // ConfirmarReservaDto, ClienteLookupDto

namespace Frontend.WebApp.Controllers
{
    public class PagoController : Controller
    {
        private readonly IHttpClientFactory _factory;
        public PagoController(IHttpClientFactory factory) => _factory = factory;

        private HttpClient CliReservas() => _factory.CreateClient("ServicioReservas");

        private HttpClient CliClientesWithToken()
        {
            var cli = _factory.CreateClient("ServicioClientes");
            var token = HttpContext.Session.GetString("token");
            if (!string.IsNullOrEmpty(token))
                cli.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return cli;
        }

        // Helpers para leer info del JWT en sesión
        private (int? idCliente, string? email) GetUserFromJwt()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token)) return (null, null);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // busca varias claves típicas
            var email = jwt.Claims.FirstOrDefault(c =>
                            c.Type == ClaimTypes.Email ||
                            c.Type == "email" ||
                            c.Type == ClaimTypes.Name ||
                            c.Type == "unique_name")?.Value;

            var idStr = jwt.Claims.FirstOrDefault(c =>
                            c.Type == "ID_Cliente" ||
                            c.Type == "IdCliente" ||
                            c.Type == ClaimTypes.NameIdentifier ||
                            c.Type == "nameid" ||
                            c.Type == "sub")?.Value;

            int? idCliente = null;
            if (int.TryParse(idStr, out var id)) idCliente = id;

            return (idCliente, email);
        }

        // 1) Stripe callback
        public async Task<IActionResult> Success(string session_id)
        {
            if (string.IsNullOrEmpty(session_id)) return View();

            var sesService = new SessionService();
            var session = sesService.Get(session_id);
            if (session.PaymentStatus != "paid") return View("Cancel");

            var piService = new Stripe.PaymentIntentService();
            var pi = piService.Get(session.PaymentIntentId);
            var md = pi.Metadata;

            // --- 1.a Tomar idCliente directamente del metadata (preferido)
            int idCliente = 0;
            if (md.TryGetValue("idCliente", out var idCliStr))
                int.TryParse(idCliStr, out idCliente);

            // --- 1.b Fallback: del JWT o del API ServicioClientes
            if (idCliente == 0)
            {
                var (idFromJwt, emailJwt) = GetUserFromJwt();
                if (idFromJwt.HasValue)
                {
                    idCliente = idFromJwt.Value;
                }
                else
                {
                    // si no hay id en JWT, intentamos por correo (del metadata o del JWT)
                    md.TryGetValue("correo", out var correoMeta);
                    var correo = !string.IsNullOrWhiteSpace(correoMeta) ? correoMeta : emailJwt;

                    if (!string.IsNullOrWhiteSpace(correo))
                    {
                        try
                        {
                            var clienteDto = await CliClientesWithToken()
                                .GetFromJsonAsync<ClienteLookupDto>(
                                    $"api/admin/cliente/correo/{Uri.EscapeDataString(correo)}",
                                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            idCliente = clienteDto?.ID_Cliente ?? 0;
                        }
                        catch
                        {
                            idCliente = 0;
                        }
                    }
                }
            }

            if (idCliente == 0)
            {
                ViewBag.Error = "No se pudo identificar el cliente (idCliente).";
                return View("Cancel");
            }

            // --- Armar DTO y guardar en Reserva/Pago
            var dto = new ConfirmarReservaDto
            {
                IdCliente = idCliente,
                IdInmueble = int.Parse(md["inmuebleId"]),
                FechaInicio = DateTime.Parse(md["fechaInicio"]),
                FechaFin = DateTime.Parse(md["fechaFin"]),
                MontoTotal = decimal.Parse(md["total"], System.Globalization.CultureInfo.InvariantCulture),
                StripePaymentId = session.PaymentIntentId
            };

            var res = await CliReservas().PostAsJsonAsync("api/reserva", dto);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                ViewBag.Error = $"Error al registrar la reserva ({(int)res.StatusCode}): {body}";
                return View("Cancel");
            }

            return View();
        }

        // 2) Crear sesión de checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearCheckoutSession(int inmuebleId, string fechaInicio, string fechaFin, decimal total, string nombreInmueble)
        {
            // Monto en centavos
            var amount = (long)Math.Round(total * 100m, MidpointRounding.AwayFromZero);
            var domain = $"{Request.Scheme}://{Request.Host}";

            // correo e idCliente desde JWT (si existen)
            var (idCliente, emailJwt) = GetUserFromJwt();

            string? email = emailJwt;
            if (string.IsNullOrWhiteSpace(email) || !MailAddress.TryCreate(email, out _))
                email = null; // no mandamos email inválido a Stripe

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = $"{domain}/Pago/Success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/Pago/Cancel",
                PaymentMethodTypes = new List<string> { "card" }, // Visa/Master/Amex en PEN
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "pen",
                            UnitAmount = amount,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Reserva: {nombreInmueble}",
                                Description = $"Del {fechaInicio} al {fechaFin}"
                            }
                        },
                        Quantity = 1
                    }
                },
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["inmuebleId"] = inmuebleId.ToString(),
                        ["fechaInicio"] = fechaInicio,
                        ["fechaFin"] = fechaFin,
                        ["total"] = total.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        ["correo"] = email ?? "",
                        ["idCliente"] = idCliente?.ToString() ?? "0"   // 🔴 guardamos el id aquí
                    }
                }
            };

            if (email != null)
                options.CustomerEmail = email;

            var service = new SessionService();
            var session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult Cancel() => View();
    }
}
