using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers;

/// <summary>
/// Endpoints de gestión de usuarios.
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    /// <summary>
    /// Registra un nuevo usuario.
    /// </summary>
    /// <param name="request">Datos del usuario a registrar (nombre, apellido, email, contraseña).</param>
    /// <returns>La representación pública del usuario creado.</returns>
    /// <response code="201">Usuario creado correctamente.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="409">El email ya está registrado.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponse>> Register([FromBody] RegisterUserRequest request)
    {
        var response = await _service.RegisterAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Autentica un usuario con email y contraseña.
    /// </summary>
    /// <param name="request">Credenciales del usuario.</param>
    /// <returns>La representación pública del usuario autenticado.</returns>
    /// <response code="200">Login exitoso.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">Credenciales incorrectas.</response>
    /// <response code="403">Cuenta bloqueada.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _service.LoginAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Obtiene un usuario por su identificador.
    /// </summary>
    /// <param name="id">Identificador único del usuario.</param>
    /// <returns>La representación pública del usuario.</returns>
    /// <response code="200">Usuario encontrado.</response>
    /// <response code="404">Usuario no encontrado.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponse>> GetById(Guid id)
    {
        var response = await _service.GetByIdAsync(id);
        return Ok(response);
    }
}
