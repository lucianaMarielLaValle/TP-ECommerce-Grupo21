namespace Cart.API.Models;

/// <summary>
/// Línea del carrito./// </summary>
public class CartItem
{
    /// <summary>Producto referenciado.</summary>
    public Guid ProductoId { get; set; }

    /// <summary>Cantidad. Requerido, mayor a 0.</summary>
    public int Cantidad { get; set; }
}
