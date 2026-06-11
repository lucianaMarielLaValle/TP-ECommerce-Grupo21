using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs;

/// <summary>
/// Item dentro del POST /api/orders. 
/// El PrecioUnitario NO llega en el request: lo captura el Service desde Products API.
/// </summary>
public class CrearOrdenItemDTO
{
    /// <summary>Producto a incluir.</summary>
    [Required(ErrorMessage = "El productoId es obligatorio.")]
    public Guid ProductoId { get; set; }

    /// <summary>Cantidad solicitada. Mayor a 0.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }
}
