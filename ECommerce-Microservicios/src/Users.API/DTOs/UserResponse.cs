namespace Users.API.DTOs;

// Vista pública del usuario. NO incluye PasswordHash ni datos internos (intentos fallidos, motivo de bloqueo).
public class UserResponse
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; }
}
