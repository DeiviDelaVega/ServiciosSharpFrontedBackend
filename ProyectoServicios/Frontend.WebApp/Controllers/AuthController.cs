using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using DNTCaptcha.Core;

namespace Frontend.WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _http;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ServicioClientes");
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
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateDNTCaptcha (ErrorMessage = "Captcha Invalido")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var res = await _http.PostAsJsonAsync("api/auth/login", dto);
            if (res.IsSuccessStatusCode)
            {
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

            ViewBag.Mensaje = "Credenciales inválidas";
            return View(dto);
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
            ViewBag.Mensaje = await res.Content.ReadAsStringAsync();
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
    }
}
