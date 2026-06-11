namespace Cart.API.Models;

/// <summary>
/// Carrito de un usuario.
/// </summary>
public class Cart
{
    /// <summary>Usuario dueño del carrito.</summary>
    public Guid UsuarioId { get; set; }

    /// <summary>Productos en el carrito.</summary>
    public List<CartItem> Items { get; set; } = [];

    /// <summary>Se actualiza automáticamente en cada operación.</summary>
    public DateTime FechaActualizacion { get; set; }
}
