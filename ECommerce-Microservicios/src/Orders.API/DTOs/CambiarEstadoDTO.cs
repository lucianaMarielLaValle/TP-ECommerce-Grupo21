using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs;

/// <summary>
/// Cuerpo del PUT /api/orders/{id}/status.
public class CambiarEstadoDTO
{
    /// <summary>Nuevo estado: "Pendiente" | "Confirmada" | "Enviada" | "Entregada" | "Cancelada".</summary>
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public string Estado { get; set; } = string.Empty;
}
