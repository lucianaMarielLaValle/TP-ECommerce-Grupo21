namespace Products.API.DTOs;

/// <summary>Datos para crear un producto.</summary>
public class CrearProductoDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string Categoria { get; set; } = string.Empty;
}

/// <summary>Datos para actualizar un producto.</summary>
public class ActualizarProductoDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string Categoria { get; set; } = string.Empty;
}