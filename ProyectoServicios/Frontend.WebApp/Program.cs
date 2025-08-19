using DNTCaptcha.Core;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Stripe;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var us = new CultureInfo("en-US");
var loc = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(us),
    SupportedCultures = new List<CultureInfo> { us },
    SupportedUICultures = new List<CultureInfo> { us },
};

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
