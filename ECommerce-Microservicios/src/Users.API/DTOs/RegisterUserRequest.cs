namespace Users.API.DTOs;

public class RegisterUserRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    // Recibe la contraseña en texto plano. El servicio la hashea antes de persistir; nunca se almacena así.
    public string Password { get; set; } = string.Empty;
}
