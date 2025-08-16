using OracleDBManager.Web.Components;
using OracleDBManager.Core.Interfaces;
using OracleDBManager.Infrastructure.Configuration;
using OracleDBManager.Infrastructure.Repositories;
using OracleDBManager.Services.Implementation;
using Microsoft.AspNetCore.Authentication.Negotiate;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configurar SignalR
builder.Services.AddSignalR();

// Configurar autenticación Windows
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization();

// Configurar Oracle
var oracleConfig = new OracleConfiguration();
builder.Configuration.GetSection("OracleConfiguration").Bind(oracleConfig);
builder.Services.AddSingleton(oracleConfig);

// Registrar servicios
builder.Services.AddScoped<ILockRepository, LockRepository>();
builder.Services.AddScoped<ILockService, LockService>();

// Configurar logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

// Agregar HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Agregar caché distribuida en memoria (requerida para sesiones)
builder.Services.AddDistributedMemoryCache();

// Configurar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireAuthorization(); // Requiere autenticación para todas las páginas

// Configurar SignalR hub cuando lo implementemos
// app.MapHub<LockHub>("/lockhub");

app.Run();
