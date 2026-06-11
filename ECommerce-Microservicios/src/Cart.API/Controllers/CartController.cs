using Cart.API.DTOs;
using Cart.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cart.API.Controllers;

/// <summary>Endpoints del carrito de compras.</summary>
[ApiController]
[Route("api/cart")]
[Tags("Cart")]
[Produces("application/json")]
public class CartController(ICartService service) : ControllerBase
{
    /// <summary>Obtiene el carrito del usuario.</summary>
    /// <response code="200">Carrito encontrado.</response>
    /// <response code="404">El usuario no tiene carrito (CRT-001).</response>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(CartResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ObtenerCarrito(Guid userId)
        => Ok(await service.ObtenerCarritoAsync(userId));

    /// <summary>Agrega un producto al carrito (suma a la cantidad existente).</summary>
    /// <response code="200">Producto agregado; devuelve el carrito actualizado.</response>
    /// <response code="400">Cantidad inválida (CRT-004).</response>
    /// <response code="404">Producto no encontrado (CRT-002).</response>
    /// <response code="422">Stock insuficiente (CRT-003).</response>
    [HttpPost("{userId:guid}/items")]
    [ProducesResponseType(typeof(CartResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AgregarItem(Guid userId, [FromBody] AgregarItemDTO dto)
        => Ok(await service.AgregarItemAsync(userId, dto));

    /// <summary>Actualiza la cantidad de un item del carrito.</summary>
    /// <response code="200">Cantidad actualizada; devuelve el carrito actualizado.</response>
    /// <response code="400">Cantidad inválida (CRT-004).</response>
    /// <response code="404">Carrito (CRT-001) o producto (CRT-002) no encontrado.</response>
    /// <response code="422">Stock insuficiente (CRT-003).</response>
    [HttpPut("{userId:guid}/items/{productId:guid}")]
    [ProducesResponseType(typeof(CartResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ActualizarCantidad(Guid userId, Guid productId, [FromBody] ActualizarCantidadDTO dto)
        => Ok(await service.ActualizarCantidadAsync(userId, productId, dto));

    /// <summary>Quita un producto del carrito.</summary>
    /// <response code="204">Producto quitado.</response>
    /// <response code="404">El usuario no tiene carrito (CRT-001).</response>
    [HttpDelete("{userId:guid}/items/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> QuitarItem(Guid userId, Guid productId)
    {
        await service.QuitarItemAsync(userId, productId);
        return NoContent();
    }

    /// <summary>Vacía el carrito completo del usuario.</summary>
    /// <response code="204">Carrito vaciado.</response>
    /// <response code="404">El usuario no tiene carrito (CRT-001).</response>
    [HttpDelete("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VaciarCarrito(Guid userId)
    {
        await service.VaciarCarritoAsync(userId);
        return NoContent();
    }
}
