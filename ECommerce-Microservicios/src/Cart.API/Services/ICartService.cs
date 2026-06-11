using Cart.API.DTOs;

namespace Cart.API.Services;

/// <summary>
/// Lógica de negocio del carrito.
/// </summary>
public interface ICartService
{
    /// <summary>GET: devuelve el carrito del usuario o lanza CRT-001 si no existe.</summary>
    Task<CartResponseDTO> ObtenerCarritoAsync(Guid usuarioId);

    /// <summary>POST: agrega un producto (suma a la cantidad existente). Crea el carrito si no existía.</summary>
    Task<CartResponseDTO> AgregarItemAsync(Guid usuarioId, AgregarItemDTO dto);

    /// <summary>PUT: fija la cantidad de un producto en el carrito (upsert del item).</summary>
    Task<CartResponseDTO> ActualizarCantidadAsync(Guid usuarioId, Guid productoId, ActualizarCantidadDTO dto);

    /// <summary>DELETE item: quita un producto del carrito.</summary>
    Task QuitarItemAsync(Guid usuarioId, Guid productoId);

    /// <summary>DELETE carrito: vacía/elimina el carrito completo.</summary>
    Task VaciarCarritoAsync(Guid usuarioId);
}
