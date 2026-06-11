using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs;

/// <summary>
/// Cuerpo del POST /api/cart/{userId}/items.
/// </summary>
public class AgregarItemDTO
{
    [Required(ErrorMessage = "El productoId es obligatorio.")]
    public Guid ProductoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }
}
