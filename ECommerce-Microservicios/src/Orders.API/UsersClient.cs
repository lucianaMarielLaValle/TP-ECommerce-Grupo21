using System.Net;

namespace Orders.API;

public class UsersClient
{
    private readonly HttpClient _httpClient;
    public UsersClient(HttpClient httpClient) => _httpClient = httpClient;

    /// <summary>Verifica si un usuario existe en Users.API. Devuelve false si responde 404.</summary>
    public async Task<bool> ExisteUsuarioAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"/api/users/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        response.EnsureSuccessStatusCode();
        return true;
    }
}