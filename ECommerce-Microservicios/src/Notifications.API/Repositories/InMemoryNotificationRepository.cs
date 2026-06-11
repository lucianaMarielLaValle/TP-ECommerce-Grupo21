using Notifications.API.Models;

namespace Notifications.API.Repositories;

public class InMemoryNotificationRepository : INotificationRepository
{
    private readonly List<Notification> _notifications = new();

    public Task<Notification> AddAsync(Notification notification)
    {
        _notifications.Add(notification);
        return Task.FromResult(notification);
    }

    public Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
    {
        var result = new List<Notification>();

        foreach (var notification in _notifications)
        {
            if (notification.UsuarioId == userId)
            {
                result.Add(notification);
            }
        }

        return Task.FromResult<IEnumerable<Notification>>(result);
    }
}