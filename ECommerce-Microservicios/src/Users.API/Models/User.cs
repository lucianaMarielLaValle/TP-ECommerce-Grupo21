namespace Users.API.Models;

public class User
{
    // Valores válidos para MotivoBloqueo
    public const string MotivoActivo = "";
    public const string MotivoIntentosFallidos = "IntentosFallidos";
    public const string MotivoFraude = "Fraude";

    // Propiedades del enunciado
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; }

    // Propiedades internas (no salen en DTOs)
    public string PasswordHash { get; set; } = string.Empty;
    public int IntentosFallidos { get; set; } = 0;
    public string MotivoBloqueo { get; set; } = MotivoActivo;
}
