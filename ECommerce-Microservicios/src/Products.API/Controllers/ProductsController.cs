using Microsoft.AspNetCore.Mvc;
using Products.API.DTOs;
using Products.API.Models;
using Products.API.Services;

namespace Products.API.Controllers;

/// <summary>Gestión del catálogo de productos.</summary>
[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _service;
    public ProductsController(ProductService service) => _service = service;

    /// <summary>Lista los productos, con filtros opcionales por categoría y nombre.</summary>
    /// <response code="200">Lista de productos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] string? categoria, [FromQuery] string? nombre)
        => Ok(await _service.GetAllAsync(categoria, nombre));

    /// <summary>Obtiene un producto por su identificador.</summary>
    /// <response code="200">Producto encontrado.</response>
    /// <response code="404">Producto no encontrado (PRD-001).</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Crea un nuevo producto.</summary>
    /// <response code="201">Producto creado.</response>
    /// <response code="400">Datos inválidos (PRD-002).</response>
    /// <response code="409">Nombre duplicado en la categoría (PRD-003).</response>
    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CrearProductoDTO request)
    {
        var product = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>Actualiza un producto existente.</summary>
    /// <response code="200">Producto actualizado.</response>
    /// <response code="400">Datos inválidos (PRD-002).</response>
    /// <response code="404">Producto no encontrado (PRD-001).</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ActualizarProductoDTO request)
        => Ok(await _service.UpdateAsync(id, request));

    /// <summary>Elimina un producto.</summary>
    /// <response code="204">Producto eliminado.</response>
    /// <response code="404">Producto no encontrado (PRD-001).</response>
    /// <response code="409">Producto con órdenes activas (PRD-004).</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}