using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.HttpClients;
using Notifications.API.Models;
using Notifications.API.Repositories;

namespace Notifications.API.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IUsersApiClient _usersApiClient;

    public NotificationService(
        INotificationRepository repository,
        IUsersApiClient usersApiClient)
    {
        _repository = repository;
        _usersApiClient = usersApiClient;
    }

    public async Task<NotificationResponse> SendAsync(SendNotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Mensaje))
        {
            throw new ValidationException("NTF-002", "El campo Mensaje es obligatorio.");
        }

        if (request.Tipo != "Email" && request.Tipo != "SMS" && request.Tipo != "Push")
        {
            throw new ValidationException("NTF-002", "El campo Tipo debe ser Email, SMS o Push.");
        }

        if (request.UsuarioId == Guid.Empty)
        {
            throw new ValidationException("NTF-002", "El campo UsuarioId es obligatorio.");
        }

        var userExists = await _usersApiClient.UserExistsAsync(request.UsuarioId);

        if (!userExists)
        {
            throw new NotFoundException("NTF-001", "El usuario indicado no existe.");
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            Mensaje = request.Mensaje,
            Tipo = request.Tipo,
            Estado = "Enviada",
            FechaEnvio = DateTime.UtcNow
        };

        var saved = await _repository.AddAsync(notification);

        return MapToResponse(saved);
    }

    public async Task<IEnumerable<NotificationResponse>> GetByUserIdAsync(Guid userId)
    {
        var notifications = await _repository.GetByUserIdAsync(userId);

        var result = new List<NotificationResponse>();

        foreach (var notification in notifications)
        {
            result.Add(MapToResponse(notification));
        }

        if (result.Count == 0)
        {
            throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario.");
        }

        return result;
    }

    private NotificationResponse MapToResponse(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            UsuarioId = notification.UsuarioId,
            Mensaje = notification.Mensaje,
            Tipo = notification.Tipo,
            Estado = notification.Estado,
            FechaEnvio = notification.FechaEnvio
        };
    }
}