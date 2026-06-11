using System.Reflection;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Orders.Api.ExceptionHandlers;
using Orders.Api.Persistence;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);


// LOGGING — Serilog 

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Orders.API")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Servicio} {Message:lj} {CorrelationId}{NewLine}{Exception}")
    .WriteTo.File(
        formatter: new JsonFormatter(renderMessage: true),
        path: "logs/orders-.json",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();


// PERSISTENCIA — SQLite + Dapper

builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<IOrderService, OrderService>();


// HTTP ENTRE SERVICIOS — IHttpClientFactory (ORD-003 valida usuario, ORD-004/005 valida producto/stock)


builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdHandler>();

builder.Services.AddHttpClient("Users", c =>
        c.BaseAddress = new Uri(builder.Configuration["Services:UsersApi"] ?? "http://localhost:5002"))
    .AddHttpMessageHandler<CorrelationIdHandler>();

builder.Services.AddHttpClient("Products", c =>
        c.BaseAddress = new Uri(builder.Configuration["Services:ProductsApi"] ?? "http://localhost:5001"))
    .AddHttpMessageHandler<CorrelationIdHandler>();


// CONTROLLERS + validación → ORD-002 (400) con errorCode en la response 

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var mensajes = context.ModelState
            .Where(kv => kv.Value?.Errors.Count > 0)
            .SelectMany(kv => kv.Value!.Errors.Select(e => e.ErrorMessage));

        var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString();

        var problem = new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "Bad Request",
            status = 400,
            detail = "La solicitud contiene datos inválidos.",
            instance = context.HttpContext.Request.Path.Value,
            errorCode = "ORD-002",
            errorMessage = string.Join("; ", mensajes),
            correlationId
        };
        return new ObjectResult(problem) { StatusCode = 400 };
    };
});


// IExceptionHandler 

builder.Services.AddExceptionHandler<NoEncontradoExceptionHandler>();
builder.Services.AddExceptionHandler<ValidacionExceptionHandler>();
builder.Services.AddExceptionHandler<ReglaNegocioExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


// HEALTH CHECKS 

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db";

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API operativa."), tags: ["live"])
    .AddCheck("sqlite-db", () =>
    {
        try
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();
            conn.ExecuteScalar<int>("SELECT 1");
            return HealthCheckResult.Healthy("SELECT 1 ejecutado OK.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("No se pudo conectar a SQLite.", ex);
        }
    }, tags: ["ready"]);


// SWAGGER 

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Orders API", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// creo las tablas al arrancar
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();

// PIPELINE

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                        ?? Guid.NewGuid().ToString();
    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers["X-Correlation-Id"] = correlationId;

    using (LogContext.PushProperty("CorrelationId", correlationId))
        await next();
});

// Manejo global de errores.
app.UseExceptionHandler();

// Log de cada request con método, ruta, status y duración.
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, _, ex) =>
        ex != null ? LogEventLevel.Error
        : httpContext.Request.Path.StartsWithSegments("/health") ? LogEventLevel.Verbose
        : LogEventLevel.Information;

    options.EnrichDiagnosticContext = (diag, httpContext) =>
        diag.Set("CorrelationId", httpContext.Items["CorrelationId"]?.ToString());
});

// Swagger solo en Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoints de Health Checks con respuesta JSON
var healthOptions = new HealthCheckOptions { ResponseWriter = EscribirRespuestaHealth };
app.MapHealthChecks("/health", healthOptions);
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = EscribirRespuestaHealth
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = EscribirRespuestaHealth
});

app.MapControllers();

app.Run();

// Helpers locales

// Respuesta JSON de los Health Checks:
static Task EscribirRespuestaHealth(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    var json = System.Text.Json.JsonSerializer.Serialize(new
    {
        status = report.Status.ToString(), 
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description
        })
    });
    return context.Response.WriteAsync(json, Encoding.UTF8);
}

// DelegatingHandler
public class CorrelationIdHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = accessor.HttpContext?.Items["CorrelationId"]?.ToString();
        if (!string.IsNullOrEmpty(correlationId))
            request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);

        return base.SendAsync(request, cancellationToken);
    }
}
