var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("ServicioClientes", client =>
{
    client.BaseAddress = new Uri("https://localhost:7100/");
});
builder.Services.AddSession();

builder.Services.AddHttpClient("ServicioInmuebles", c =>
    c.BaseAddress = new Uri("https://localhost:7014/"));

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    //Aquí dentro va la ruta por defecto
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Auth}/{action=Login}/{id?}");

    // (Opcional) Imprimir rutas disponibles para debug
    var routeEndpoints = endpoints.DataSources
        .SelectMany(ds => ds.Endpoints)
        .OfType<RouteEndpoint>();

    foreach (var endpoint in routeEndpoints)
    {
        Console.WriteLine($"Ruta disponible: {endpoint.RoutePattern.RawText}");
    }
});

app.Run();
