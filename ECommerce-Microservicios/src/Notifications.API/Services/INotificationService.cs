using Notifications.API.DTOs;

namespace Notifications.API.Services;

public interface INotificationService
{
    Task<NotificationResponse> SendAsync(SendNotificationRequest request);
    Task<IEnumerable<NotificationResponse>> GetByUserIdAsync(Guid userId);
}