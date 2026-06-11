using Microsoft.AspNetCore.Mvc;
using Orders.API.DTOs;
using Orders.API.Models;
using Orders.API.Services;

namespace Orders.API.Controllers;

/// <summary>Gestión de órdenes de compra.</summary>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _service;
    public OrdersController(OrderService service) => _service = service;

    /// <summary>Lista las órdenes, con filtro opcional por usuario.</summary>
    /// <response code="200">Lista de órdenes.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? usuarioId)
        => Ok(await _service.GetAllAsync(usuarioId));

    /// <summary>Obtiene el detalle de una orden.</summary>
    /// <response code="200">Orden encontrada.</response>
    /// <response code="404">Orden no encontrada (ORD-001).</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Crea una nueva orden.</summary>
    /// <response code="201">Orden creada.</response>
    /// <response code="400">Datos inválidos (ORD-002).</response>
    /// <response code="404">Usuario (ORD-003) o producto (ORD-004) no encontrado.</response>
    /// <response code="422">Stock insuficiente (ORD-005).</response>
    [HttpPost]
    [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CrearOrdenDTO request)
    {
        var order = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>Actualiza el estado de una orden.</summary>
    /// <response code="200">Estado actualizado.</response>
    /// <response code="400">Estado inválido (ORD-002).</response>
    /// <response code="404">Orden no encontrada (ORD-001).</response>
    /// <response code="409">Transición de estado inválida (ORD-006).</response>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoDTO request)
        => Ok(await _service.CambiarEstadoAsync(id, request));
}