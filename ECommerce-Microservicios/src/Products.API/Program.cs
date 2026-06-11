using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Products.API;
using Products.API.ExceptionHandlers;
using Products.API.Services;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.Reflection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);


// Dapper: manejar Guid como texto en SQLite
Dapper.SqlMapper.AddTypeHandler(new GuidTypeHandler());

// ---------- Serilog ----------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Products.API")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(new JsonFormatter(),
        path: "logs/products-.log",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ---------- Servicios ----------
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Products API",
        Version = "v1",
        Description = "Microservicio de catálogo de productos - TP E-Commerce."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

// Persistencia
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<ProductService>();

// Manejo de errores (específicos primero, global al final)
builder.Services.AddExceptionHandler<NoEncontradoExceptionHandler>();
builder.Services.AddExceptionHandler<ValidacionExceptionHandler>();
builder.Services.AddExceptionHandler<ReglaNegocioExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<SqliteHealthCheck>("sqlite-db", tags: new[] { "ready" });

// Cliente HTTP hacia Orders (para validar PRD-004)
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdHandler>();
builder.Services.AddHttpClient<OrdersClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ServiciosExternos:OrdersApiUrl"]!);
    }).AddHttpMessageHandler<CorrelationIdHandler>();

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

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSerilogRequestLogging();   // loggea inicio/fin de cada request con duración

app.UseExceptionHandler();

// Health endpoints
app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = WriteHealthResponse });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponse
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteHealthResponse
});

app.MapControllers();

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