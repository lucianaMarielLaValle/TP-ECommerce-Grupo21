using Notifications.API.ExceptionHandlers;
using Notifications.API.HttpClients;
using Notifications.API.Repositories;
using Notifications.API.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/notifications-.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [CorrelationId:{CorrelationId}] {Message:lj} {Exception}{NewLine}");
});

// Servicios MVC + Swagger
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

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Inyección de dependencias del dominio
builder.Services.AddSingleton<INotificationRepository, InMemoryNotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Cliente HTTP para Users API
builder.Services.AddTransient<CorrelationIdHandler>();
builder.Services.AddHttpClient<IUsersApiClient, UsersApiClient>(client =>
{
    var usersApiUrl = builder.Configuration["UsersApi:BaseUrl"]
        ?? "http://localhost:5002/";
    client.BaseAddress = new Uri(usersApiUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddHttpMessageHandler<CorrelationIdHandler>();

// Manejadores de excepciones (orden importa: del más específico al más general)
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<Notifications.API.Middleware.CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Endpoints de health
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true
});

app.Run();