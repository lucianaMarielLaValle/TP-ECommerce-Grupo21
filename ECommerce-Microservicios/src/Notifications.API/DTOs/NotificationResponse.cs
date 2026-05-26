namespace Notifications.API.DTOs;

/// <summary>
/// Representación de una notificación registrada.
/// </summary>
public class NotificationResponse
{
    /// <summary>
    /// Identificador único de la notificación.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador único del usuario destinatario.
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Texto del mensaje enviado.
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de notificación (Email, SMS, Push).
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Estado actual de la notificación. Siempre "Enviada" al registrarse.
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora en UTC en que se envió la notificación.
    /// </summary>
    public DateTime FechaEnvio { get; set; }
}