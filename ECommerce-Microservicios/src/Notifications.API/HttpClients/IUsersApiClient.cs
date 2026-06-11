namespace Notifications.API.HttpClients;

public interface IUsersApiClient
{
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}