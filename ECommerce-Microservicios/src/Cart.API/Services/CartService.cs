using System.Net;
using System.Text.Json;
using Cart.API.DTOs;
using Cart.API.Exceptions;
using Cart.API.Models;
using Cart.API.Repositories;

namespace Cart.API.Services;

/// <summary>
/// Reglas del carrito. Valida el producto y el stock consultando Products API por HTTP
/// </summary>
public class CartService(
    ICartRepository repository,
    IHttpClientFactory httpClientFactory,
    ILogger<CartService> logger) : ICartService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public async Task<CartResponseDTO> ObtenerCarritoAsync(Guid usuarioId)
    {
        var cart = await repository.GetByUsuarioIdAsync(usuarioId)
                ?? throw new NoEncontradoException("CRT-001", "Carrito no encontrado.");
        return MapToResponse(cart);
    }

    public async Task<CartResponseDTO> AgregarItemAsync(Guid usuarioId, AgregarItemDTO dto)
    {
        // Carrito actual o uno nuevo si el usuario todavía no tenía.
        var cart = await repository.GetByUsuarioIdAsync(usuarioId)
                ?? new Models.Cart { UsuarioId = usuarioId, Items = [] };

        var existente = cart.Items.FirstOrDefault(i => i.ProductoId == dto.ProductoId);
        // POST suma a lo que ya hubiera en el carrito.
        var cantidadResultante = (existente?.Cantidad ?? 0) + dto.Cantidad;

        await ValidarProductoYStockAsync(dto.ProductoId, cantidadResultante);

        if (existente is not null)
            existente.Cantidad = cantidadResultante;
        else
            cart.Items.Add(new CartItem { ProductoId = dto.ProductoId, Cantidad = dto.Cantidad });

        cart.FechaActualizacion = DateTime.UtcNow;
        await repository.SaveAsync(cart);

        logger.LogInformation("Producto {ProductoId} agregado al carrito de {UsuarioId}.", dto.ProductoId, usuarioId);
        return MapToResponse(cart);
    }

    public async Task<CartResponseDTO> ActualizarCantidadAsync(Guid usuarioId, Guid productoId, ActualizarCantidadDTO dto)
    {
        var cart = await repository.GetByUsuarioIdAsync(usuarioId)
                ?? throw new NoEncontradoException("CRT-001", "Carrito no encontrado.");

        // PUT fija la cantidad: se valida el stock contra la cantidad nueva.
        await ValidarProductoYStockAsync(productoId, dto.Cantidad);

        var existente = cart.Items.FirstOrDefault(i => i.ProductoId == productoId);
        if (existente is not null)
            existente.Cantidad = dto.Cantidad;
        else
            cart.Items.Add(new CartItem { ProductoId = productoId, Cantidad = dto.Cantidad });

        cart.FechaActualizacion = DateTime.UtcNow;
        await repository.SaveAsync(cart);

        logger.LogInformation("Cantidad del producto {ProductoId} actualizada en el carrito de {UsuarioId}.",
            productoId, usuarioId);
        return MapToResponse(cart);
    }

    public async Task QuitarItemAsync(Guid usuarioId, Guid productoId)
    {
        var cart = await repository.GetByUsuarioIdAsync(usuarioId)
                ?? throw new NoEncontradoException("CRT-001", "Carrito no encontrado.");

        cart.Items.RemoveAll(i => i.ProductoId == productoId);
        cart.FechaActualizacion = DateTime.UtcNow;
        await repository.SaveAsync(cart);

        logger.LogInformation("Producto {ProductoId} quitado del carrito de {UsuarioId}.", productoId, usuarioId);
    }

    public async Task VaciarCarritoAsync(Guid usuarioId)
    {
        var cart = await repository.GetByUsuarioIdAsync(usuarioId)
                ?? throw new NoEncontradoException("CRT-001", "Carrito no encontrado.");

        await repository.DeleteAsync(cart.UsuarioId);
        logger.LogInformation("Carrito del usuario {UsuarioId} vaciado.", usuarioId);
    }

    // ── Integración con Products API ──

    /// <summary>
    /// Consulta GET /api/products/{id}.
    /// </summary>
    private async Task ValidarProductoYStockAsync(Guid productoId, int cantidad)
    {
        var client = httpClientFactory.CreateClient("Products");
        var response = await client.GetAsync($"/api/products/{productoId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new NoEncontradoException("CRT-002", "Producto no encontrado.");

        // Cualquier otro error de Products API se trata como error interno (CRT-005).
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        var producto = JsonSerializer.Deserialize<ProductoInfo>(body, JsonOpts)
                    ?? throw new NoEncontradoException("CRT-002", "Producto no encontrado.");

        if (cantidad > producto.Stock)
            throw new ReglaNegocioException("CRT-003",
                $"Stock insuficiente. Disponible: {producto.Stock}, solicitado: {cantidad}.", 422);
    }

    private static CartResponseDTO MapToResponse(Models.Cart cart) => new()
    {
        UsuarioId = cart.UsuarioId,
        FechaActualizacion = cart.FechaActualizacion,
        Items = cart.Items.Select(i => new CartItemResponseDTO
        {
            ProductoId = i.ProductoId,
            Cantidad = i.Cantidad
        }).ToList()
    };

    // Proyección mínima de la response de Products API.
    private sealed record ProductoInfo(Guid Id, string Nombre, int Stock);
}
