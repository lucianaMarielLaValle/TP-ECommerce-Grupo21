namespace Orders.Api.DTOs;

/// <summary>
/// Item tal como se devuelve en las responses de orden.
/// </summary>
public class OrdenItemResponseDTO
{
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
