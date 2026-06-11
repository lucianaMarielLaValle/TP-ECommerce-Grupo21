using System.Net.Http.Json;

namespace Products.API;

public class OrdersClient
{
    private readonly HttpClient _httpClient;
    public OrdersClient(HttpClient httpClient) => _httpClient = httpClient;

    /// <summary>Pregunta a Orders.API si el producto está en órdenes activas.</summary>
    public async Task<bool> TieneOrdenesActivasAsync(Guid productoId)
    {
        var response = await _httpClient.GetAsync($"/api/orders/producto/{productoId}/activas");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<bool>();
    }
}