namespace Orders.API.DTOs;

/// <summary>
/// Response de PUT /api/orders/{id}/status.
/// Reproduce el body documentado en la sección 4.3: { id, estado, fechaActualizacion }.
/// Nota: FechaActualizacion no existe en el modelo de dominio; es propia de esta response.
/// </summary>
public class CambiarEstadoResponseDTO
{
    public Guid Id { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaActualizacion { get; set; }
}
