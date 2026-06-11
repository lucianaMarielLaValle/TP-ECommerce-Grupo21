namespace Cart.API.DTOs;

/// <summary>
/// Response de GET /api/cart/{userId}, POST y PUT de items.
/// </summary>
public class CartResponseDTO
{
    public Guid UsuarioId { get; set; }
    public List<CartItemResponseDTO> Items { get; set; } = [];
    public DateTime FechaActualizacion { get; set; }
}
