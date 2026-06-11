namespace Cart.API.DTOs;

/// <summary>Item tal como se devuelve en la response del carrito.</summary>
public class CartItemResponseDTO
{
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
}
