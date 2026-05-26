using Notifications.API.Exceptions;

namespace Notifications.API.HttpClients;

public class UsersApiClient : IUsersApiClient
{
    private readonly HttpClient _httpClient;

    public UsersApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/users/{userId}", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            throw new BusinessRuleException(
                "NTF-005",
                $"Users API respondió con un código inesperado: {(int)response.StatusCode}.");
        }
        catch (HttpRequestException ex)
        {
            throw new BusinessRuleException(
                "NTF-005",
                $"No se pudo contactar a Users API: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            throw new BusinessRuleException(
                "NTF-005",
                "La llamada a Users API excedió el tiempo de espera.");
        }
    }
}