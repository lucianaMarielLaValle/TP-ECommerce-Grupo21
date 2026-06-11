namespace Orders.API.DTOs;

/// <summary>
/// Response de GET /api/orders, GET /api/orders/{id} y POST /api/orders.
/// Reproduce exactamente el body documentado en la sección 4.3 del enunciado.
/// </summary>
public class OrdenResponseDTO
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public List<OrdenItemResponseDTO> Items { get; set; } = [];
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}
