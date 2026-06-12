using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Notifications.API;
using Notifications.API.ExceptionHandlers;
using Notifications.API.HttpClients;
using Notifications.API.Repositories;
using Notifications.API.Services;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.Reflection;
using System.Text.Json;

// Dapper: manejar Guid como texto en SQLite
Dapper.SqlMapper.AddTypeHandler(new GuidTypeHandler());

var builder = WebApplication.CreateBuilder(args);

// ---------- Serilog ----------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Notifications.API")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(new JsonFormatter(),
        path: "logs/notifications-.log",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ---------- Servicios MVC + Swagger ----------
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Notifications API",
        Version = "v1",
        Description = "Microservicio de notificaciones del e-commerce. Registra y consulta notificaciones por usuario."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// ---------- Persistencia ----------
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<INotificationRepository, SqliteNotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// ---------- Cliente HTTP para Users API ----------
builder.Services.AddTransient<CorrelationIdHandler>();
builder.Services.AddHttpClient<IUsersApiClient, UsersApiClient>(client =>
{
    var usersApiUrl = builder.Configuration["UsersApi:BaseUrl"]
        ?? "http://localhost:5001/";
    client.BaseAddress = new Uri(usersApiUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddHttpMessageHandler<CorrelationIdHandler>();

// ---------- Manejadores de excepciones (orden importa: del más específico al más general) ----------
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ---------- Health Checks ----------
builder.Services.AddHealthChecks()
    .AddCheck<SqliteHealthCheck>("sqlite-db", tags: new[] { "ready" });

var app = builder.Build();

// Crea la tabla al arrancar
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();

// ---------- Pipeline ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<Notifications.API.Middleware.CorrelationIdMiddleware>();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Endpoints de health
app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = WriteHealthResponse });
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteHealthResponse
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponse
});

app.Run();

// Respuesta JSON de los health checks (Healthy | Degraded | Unhealthy)
static Task WriteHealthResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    var result = JsonSerializer.Serialize(new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description
        }),
        totalDuration = report.TotalDuration.ToString()
    });
    return context.Response.WriteAsync(result);
}
