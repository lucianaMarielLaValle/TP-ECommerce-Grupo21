namespace Orders.Api.Models;

/// <summary>
/// Entidad de dominio que representa una orden de compra.
/// </summary>
public class Order
{
    /// <summary>Identificador único de la orden.</summary>
    public Guid Id { get; set; }

    /// <summary>Usuario que realizó la orden.</summary>
    public Guid UsuarioId { get; set; }

    /// <summary>Productos incluidos en la orden.</summary>
    public List<OrderItem> Items { get; set; } = [];

    /// <summary>Importe total. Lo calcula el Service al crear la orden.</summary>
    public decimal Total { get; set; }

    /// <summary>"Pendiente" | "Confirmada" | "Enviada" | "Entregada" | "Cancelada".</summary>
    public string Estado { get; set; } = "Pendiente";

    /// <summary>Fecha de creación. Se asigna automáticamente al crear.</summary>
    public DateTime FechaCreacion { get; set; }
}
