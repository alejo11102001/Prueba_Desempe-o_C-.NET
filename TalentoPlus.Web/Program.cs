using Microsoft.EntityFrameworkCore;
using TalentoPlus.Core.Interfaces;
using TalentoPlus.Infrastructure.Data;
using TalentoPlus.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar la Conexión a Base de Datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Configurar Identity (Login de Administrador)
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
        options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 3. Inyectar el Repositorio (IMPORTANTE)
// Cuando el controlador pida IEmpleadoRepository, dale EmpleadoRepository
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurar el pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Quién eres
app.UseAuthorization();  // Qué puedes hacer

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // Necesario para el Login de Identity

app.Run();