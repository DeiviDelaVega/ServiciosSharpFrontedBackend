using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Frontend.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace Frontend.WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _http;
        private readonly HttpClient _recaptcha;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<AuthController> logger)
        {
            _http = httpClientFactory.CreateClient("ServicioClientes");
            _recaptcha = httpClientFactory.CreateClient("Recaptcha");
            _config = config;
            _logger = logger;
        }


        [HttpGet]
        public IActionResult Login()
        {
            var tokenString = HttpContext.Session.GetString("token");
            if (!string.IsNullOrEmpty(tokenString))
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(tokenString);
                var rol = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (rol == "admin") return RedirectToAction("Index", "Admin");
                if (rol == "cliente") return RedirectToAction("Index", "Cliente");
            }

            return View(new LoginVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var recaptchaToken = Request.Form["g-recaptcha-response"].ToString();

            var captcha = await VerifyRecaptchaAsync(recaptchaToken);
            if (!captcha.ok)
            {
                var detail = captcha.errors.Length > 0 ? string.Join(", ", captcha.errors) : captcha.raw;
                ModelState.AddModelError(string.Empty, $"Captcha inválido ({detail})");
                return View(vm);
            }

            var dto = new LoginDto { Correo = vm.Correo, Clave = vm.Clave };
            var res = await _http.PostAsJsonAsync("api/auth/login", dto);

            if (!res.IsSuccessStatusCode)
            {
                ViewBag.Mensaje = "Credenciales inválidas";
                return View(vm);
            }

            var tokenObj = await res.Content.ReadFromJsonAsync<TokenResponse>();
            HttpContext.Session.SetString("token", tokenObj!.Token!);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenObj.Token);

            var rol = token.Claims.First(c => c.Type == ClaimTypes.Role).Value;
            var nombreCompleto = token.Claims.FirstOrDefault(c => c.Type == "NombreCompleto")?.Value;
            HttpContext.Session.SetString("nombreUsuario", nombreCompleto ?? "");

            return rol == "admin"
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Cliente");
        }


        private async Task<(bool ok, string[] errors, string raw)> VerifyRecaptchaAsync(string responseToken)
        {
            if (string.IsNullOrWhiteSpace(responseToken))
                return (false, new[] { "missing-input-response" }, "");

            var secret = _config["Recaptcha:SecretKey"];
            if (string.IsNullOrWhiteSpace(secret))
                return (false, new[] { "missing-secret" }, "");

            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["secret"] = secret,
                ["response"] = responseToken
            });

            var res = await _recaptcha.PostAsync("siteverify", content);
            var raw = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                return (false, new[] { $"http-{(int)res.StatusCode}" }, raw);

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            var ok = root.TryGetProperty("success", out var s) && s.ValueKind == JsonValueKind.True;

            string[] errors = Array.Empty<string>();
            if (root.TryGetProperty("error-codes", out var e) && e.ValueKind == JsonValueKind.Array)
            {
                errors = e.EnumerateArray()
                    .Select(x => x.GetString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray()!;
            }

            return (ok, errors, raw);
        }


        [HttpGet]
        public IActionResult RegistroCliente()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistroCliente(ClienteDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var res = await _http.PostAsJsonAsync("api/auth/register-cliente", dto);
            if (res.IsSuccessStatusCode)
            {
                var mensajeApi = await res.Content.ReadAsStringAsync();
                ViewBag.Mensaje = mensajeApi + " - 📧 Correo enviado";
            }
            else
            {
                ViewBag.Error = await res.Content.ReadAsStringAsync();
            }
            return View(dto);
        }

        [HttpGet]
        public IActionResult RegistroAdmin()
        {
            var token = HttpContext.Session.GetString("token");
            if (token == null) return RedirectToAction("Login");

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var rol = jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value;

            if (rol != "admin")
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistroAdmin(AdministradorDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var token = HttpContext.Session.GetString("token");
            if (token == null) return RedirectToAction("Login");

            var req = new HttpRequestMessage(HttpMethod.Post, "api/auth/register-admin")
            {
                Content = JsonContent.Create(dto)
            };
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var res = await _http.SendAsync(req);
            ViewBag.Mensaje = await res.Content.ReadAsStringAsync();
            return View(dto);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["LogoutMessage"] = "Se cerró sesión correctamente"; // Guardar el mensaje para mostrarlo en la vista
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult PaginaInicio()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.IsAuthenticated = false;
                return View();
            }

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var rol = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var nombreUsuario = jwt.Claims.FirstOrDefault(c => c.Type == "NombreCompleto")?.Value;

            ViewBag.IsAuthenticated = true;
            ViewBag.Role = rol;
            ViewBag.NombreUsuario = nombreUsuario;

            return View();
        }

        [HttpGet]
        public IActionResult ContinuarNavegando()
        {
            var token = HttpContext.Session.GetString("token");
            if (token == null) return RedirectToAction("Login");

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var rol = jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value;

            if (rol == "admin") return RedirectToAction("Index", "Admin");
            if (rol == "cliente") return RedirectToAction("Index", "Cliente");

            return RedirectToAction("PaginaInicio");
        }
    }
    public class RecaptchaVerifyResponse
    {
        public bool Success { get; set; }
        public string[]? ErrorCodes { get; set; }
        public string? Hostname { get; set; }
        public DateTime? ChallengeTs { get; set; }
    }
}
