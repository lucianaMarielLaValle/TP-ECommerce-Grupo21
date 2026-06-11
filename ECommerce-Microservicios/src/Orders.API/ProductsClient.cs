using System.Net;
using System.Net.Http.Json;

namespace Orders.API;

/// <summary>Producto tal como lo devuelve Products.API (solo los campos que usamos).</summary>
public class ProductoExterno
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}

public class ProductsClient
{
    private readonly HttpClient _httpClient;
    public ProductsClient(HttpClient httpClient) => _httpClient = httpClient;

    /// <summary>Obtiene un producto. Devuelve null si Products.API responde 404.</summary>
    public async Task<ProductoExterno?> ObtenerProductoAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"/api/products/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductoExterno>();
    }
}