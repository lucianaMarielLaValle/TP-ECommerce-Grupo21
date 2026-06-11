using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs;

/// <summary>
/// Cuerpo del PUT /api/cart/{userId}/items/{productId}.
/// </summary>
public class ActualizarCantidadDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }
}
