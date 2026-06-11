namespace Orders.API.Models;

/// <summary>
/// Línea de detalle de una orden.
/// </summary>
public class OrderItem
{
    /// <summary>Producto referenciado.</summary>
    public Guid ProductoId { get; set; }

    /// <summary>Cantidad solicitada. Requerido, mayor a 0.</summary>
    public int Cantidad { get; set; }

    /// <summary>Precio del producto capturado al momento de crear la orden.</summary>
    public decimal PrecioUnitario { get; set; }
}
