using Cart.API.Models;

namespace Cart.API.Repositories;

/// <summary>
/// Contrato del repositorio de carritos.
/// </summary>
public interface ICartRepository
{
    Task<Models.Cart?> GetByUsuarioIdAsync(Guid usuarioId);
    Task SaveAsync(Models.Cart cart);
    Task DeleteAsync(Guid usuarioId);
}
