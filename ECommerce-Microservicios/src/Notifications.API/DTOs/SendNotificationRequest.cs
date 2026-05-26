namespace Notifications.API.DTOs;

/// <summary>
/// Datos requeridos para registrar una notificación.
/// </summary>
public class SendNotificationRequest
{
    /// <summary>
    /// Identificador único del usuario destinatario.
    /// </summary>
    /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Texto del mensaje a enviar. No puede estar vacío.
    /// </summary>
    /// <example>Su orden #f1e2d3c4 fue confirmada.</example>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de notificación. Valores válidos: Email, SMS, Push.
    /// </summary>
    /// <example>Email</example>
    public string Tipo { get; set; } = string.Empty;
}