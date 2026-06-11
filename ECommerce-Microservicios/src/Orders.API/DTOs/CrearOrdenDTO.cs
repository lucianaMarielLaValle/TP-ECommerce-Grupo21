using System.ComponentModel.DataAnnotations;

namespace Orders.Api.DTOs;

/// <summary>
/// Cuerpo del POST /api/orders. 
/// La validación por Data Annotations produce el error ORD-002 (400) ante datos faltantes o inválidos.
/// </summary>
public class CrearOrdenDTO
{
    /// <summary>Usuario que realiza la orden.</summary>
    [Required(ErrorMessage = "El usuarioId es obligatorio.")]
    public Guid UsuarioId { get; set; }

    /// <summary>Lista de items. No puede estar vacía.</summary>
    [Required(ErrorMessage = "La orden debe incluir al menos un item.")]
    [MinLength(1, ErrorMessage = "La orden debe incluir al menos un item.")]
    public List<CrearOrdenItemDTO> Items { get; set; } = [];
}
