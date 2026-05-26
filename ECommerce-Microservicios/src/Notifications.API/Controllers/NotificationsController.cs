using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Controllers;

/// <summary>
/// Endpoints de gestión de notificaciones.
/// </summary>
[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service)
    {
        _service = service;
    }

    /// <summary>
    /// Registra una nueva notificación para un usuario.
    /// </summary>
    /// <param name="request">Datos de la notificación a registrar.</param>
    /// <returns>La notificación creada con su Id, estado y fecha de envío.</returns>
    /// <response code="200">Notificación registrada correctamente.</response>
    /// <response code="400">El request es inválido (campos faltantes o con formato incorrecto).</response>
    /// <response code="404">El usuario indicado no existe.</response>
    /// <response code="422">No se pudo contactar a Users API o el servicio está degradado.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpPost("send")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotificationResponse>> Send([FromBody] SendNotificationRequest request)
    {
        var response = await _service.SendAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Lista todas las notificaciones registradas para un usuario.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>Lista de notificaciones del usuario (puede estar vacía).</returns>
    /// <response code="200">Listado obtenido correctamente.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetByUserId(Guid userId)
    {
        var notifications = await _service.GetByUserIdAsync(userId);
        return Ok(notifications);
    }
}