using System.ComponentModel.DataAnnotations;

namespace Products.API.Models;

/// <summary>Representa un producto del catálogo.</summary>
public class Product
{
    /// <summary>Identificador único del producto.</summary>
    public Guid Id { get; set; }

    /// <summary>Nombre del producto. Requerido, máx. 100 caracteres.</summary>
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Descripción del producto. Opcional, máx. 500 caracteres.</summary>
    [MaxLength(500)]
    public string? Descripcion { get; set; }

    /// <summary>Precio del producto. Requerido, mayor a 0.</summary>
    public decimal Precio { get; set; }

    /// <summary>Stock disponible. Requerido, mayor o igual a 0.</summary>
    public int Stock { get; set; }

    /// <summary>Categoría del producto. Requerido.</summary>
    [Required]
    public string Categoria { get; set; } = string.Empty;

    /// <summary>Fecha de creación, asignada automáticamente.</summary>
    public DateTime FechaCreacion { get; set; }
}