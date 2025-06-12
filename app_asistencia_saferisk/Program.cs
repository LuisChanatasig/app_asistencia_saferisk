using app_asistencia_saferisk.Models;
using app_asistencia_saferisk.Servicios;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//// Configuración de la base de datos
builder.Services.AddDbContext<AppAsistenciaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("conexion")));
//Servicios
builder.Services.AddScoped<AutenticacionService>();
builder.Services.AddScoped<JornadaService>();
builder.Services.AddScoped<ReporteService>();

//Configuracion tiempo de sesion
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10); // Cambia el valor a lo que prefieras
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddLogging();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Agregar esta línea ANTES de UseAuthorization
app.UseSession();

// Configuración de endpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Autenticacion}/{action=Login}/{id?}");
});
app.Run();
