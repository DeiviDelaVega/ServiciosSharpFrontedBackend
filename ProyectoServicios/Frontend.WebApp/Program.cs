using System.Globalization;
using DNTCaptcha.Core;
using Microsoft.AspNetCore.Localization;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

var us = new CultureInfo("en-US");
var loc = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(us),
    SupportedCultures = new List<CultureInfo> { us },
    SupportedUICultures = new List<CultureInfo> { us },
};


builder.Services.AddHttpClient("ServicioReservas", c =>
{
    c.BaseAddress = new Uri("https://localhost:7185/"); // URL de tu ServicioReservas.API
});


StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("ServicioClientes", c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiUrls:ServicioClientes"]!));
builder.Services.AddHttpClient("ServicioInmuebles", c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiUrls:ServicioInmuebles"]!));
builder.Services.AddHttpClient("ServicioReservas", c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiUrls:ServicioReservas"]!));

builder.Services.AddDNTCaptcha(o =>
{
    o.UseSessionStorageProvider();
    o.WithEncryptionKey("mysupersecret_dntcaptcha_encryption_key_2025");
    o.ShowThousandsSeparators(false);
    o.AbsoluteExpiration(minutes: 7);

});
// Session config
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(3);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});


var app = builder.Build();

app.UseSession();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRequestLocalization(loc);
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapPost("/api/chat", async (HttpContext ctx, IHttpClientFactory httpFactory) =>
    {
        var req = await ctx.Request.ReadFromJsonAsync<ChatRequest>();
        if (req is null || string.IsNullOrWhiteSpace(req.Message))
            return Results.BadRequest(new { error = "message requerido" });

        // ✅ poné tu webhook real en appsettings y léelo por config (mejor)
        var n8nUrl = builder.Configuration["N8N:ChatWebhookUrl"];
        if (string.IsNullOrWhiteSpace(n8nUrl))
            return Results.Problem("Falta config N8N:ChatWebhookUrl");

        var http = httpFactory.CreateClient();

        // Lo que n8n recibe (ajustalo al JSON que esperes en tu workflow)
        var n8nBody = new
        {
            conversationId = req.ConversationId,
            message = req.Message
        };

        var n8nRes = await http.PostAsJsonAsync(n8nUrl, n8nBody);
        var n8nText = await n8nRes.Content.ReadAsStringAsync();

        if (!n8nRes.IsSuccessStatusCode)
            return Results.Problem($"n8n respondió {((int)n8nRes.StatusCode)}: {n8nText}");
        string ExtractReplyFromN8n(string rawJson)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
                var root = doc.RootElement;

                // Caso n8n: [ { "output": { "response": "..." } } ]
                if (root.ValueKind == System.Text.Json.JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var first = root[0];

                    if (first.TryGetProperty("output", out var output) &&
                        output.TryGetProperty("response", out var responseProp) &&
                        responseProp.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        return responseProp.GetString() ?? "";
                    }
                }

                // Caso alternativo: { reply: "..." }
                if (root.ValueKind == System.Text.Json.JsonValueKind.Object &&
                    root.TryGetProperty("reply", out var replyProp) &&
                    replyProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    return replyProp.GetString() ?? "";
                }

                // Fallback: si no matchea nada
                return rawJson;
            }
            catch
            {
                return rawJson;
            }
        }

        var reply = ExtractReplyFromN8n(n8nText);
        return Results.Ok(new { conversationId = req.ConversationId, reply });

    });

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Auth}/{action=PaginaInicio}/{id?}");

    var routeEndpoints = endpoints.DataSources
        .SelectMany(ds => ds.Endpoints)
        .OfType<RouteEndpoint>();

    foreach (var endpoint in routeEndpoints)
    {
        Console.WriteLine($"Ruta disponible: {endpoint.RoutePattern.RawText}");
    }
});


app.Run();
public record ChatRequest(string? ConversationId, string Message);
