using Notifications.API.Models;

namespace Notifications.API.Repositories;

public interface INotificationRepository
{
    Task<Notification> AddAsync(Notification notification);
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
}