using OracleDBManager.Web.Components;
using OracleDBManager.Core.Interfaces;
using OracleDBManager.Infrastructure.Configuration;
using OracleDBManager.Infrastructure.Repositories;
using OracleDBManager.Services.Implementation;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Serilog;
using Serilog.Events;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/oracle-dbmanager-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
        fileSizeLimitBytes: 50 * 1024 * 1024, // 50MB
        retainedFileCountLimit: 30)
    .WriteTo.File(
        path: "Logs/oracle-errors-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
        restrictedToMinimumLevel: LogEventLevel.Error,
        fileSizeLimitBytes: 50 * 1024 * 1024, // 50MB
        retainedFileCountLimit: 90) // Mantener errores por 3 meses
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Usar Serilog
builder.Host.UseSerilog();

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
builder.Services.AddScoped<IPerformanceRepository, PerformanceRepository>();
builder.Services.AddScoped<IPerformanceService, PerformanceService>();
builder.Services.AddScoped<IConnectionLogService, ConnectionLogService>();



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

try
{
    Log.Information("Iniciando Oracle Database Manager Web Host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminado inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
