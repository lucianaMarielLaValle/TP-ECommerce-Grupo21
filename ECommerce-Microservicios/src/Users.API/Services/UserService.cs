using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;
using Users.API.Repositories;

namespace Users.API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserResponse> RegisterAsync(RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            throw new ValidationException("USR-002", "El campo Nombre es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Apellido))
        {
            throw new ValidationException("USR-002", "El campo Apellido es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("USR-002", "El campo Email es obligatorio.");
        }

        // Formato básico: debe contener "@" y un "." después del "@".
        var atIndex = request.Email.IndexOf('@');
        if (atIndex < 0 || request.Email.IndexOf('.', atIndex) < 0)
        {
            throw new ValidationException("USR-002", "El email tiene un formato inválido.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("USR-002", "El campo Password es obligatorio.");
        }

        if (request.Password.Length < 6)
        {
            throw new ValidationException("USR-002", "La contraseña debe tener al menos 6 caracteres.");
        }

        var existing = await _repository.GetByEmailAsync(request.Email);
        if (existing != null)
        {
            throw new BusinessRuleException("USR-001", "Ya existe un usuario con ese email.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FechaRegistro = DateTime.UtcNow,
            Activo = true,
            IntentosFallidos = 0,
            MotivoBloqueo = User.MotivoActivo
        };

        var savedUser = await _repository.AddAsync(user);
        return MapToResponse(savedUser);
    }

    public async Task<UserResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("USR-002", "Email y Password son obligatorios.");
        }

        var user = await _repository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            // Mensaje genérico a propósito: no revelamos si el usuario existe o no.
            throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");
        }

        if (!user.Activo)
        {
            if (user.MotivoBloqueo == User.MotivoIntentosFallidos)
            {
                throw new ForbiddenException("USR-004", "La cuenta está bloqueada por intentos fallidos.");
            }

            if (user.MotivoBloqueo == User.MotivoFraude)
            {
                throw new ForbiddenException("USR-005", "La cuenta está bloqueada por sospecha de fraude.");
            }

            throw new ForbiddenException("USR-004", "La cuenta está bloqueada.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.IntentosFallidos++;

            var bloqueadoEnEsteIntento = false;
            if (user.IntentosFallidos >= 3)
            {
                user.Activo = false;
                user.MotivoBloqueo = User.MotivoIntentosFallidos;
                bloqueadoEnEsteIntento = true;
            }

            await _repository.UpdateAsync(user);

            if (bloqueadoEnEsteIntento)
            {
                throw new ForbiddenException("USR-004", "La cuenta ha sido bloqueada por superar los intentos permitidos.");
            }

            throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");
        }

        if (user.IntentosFallidos > 0)
        {
            user.IntentosFallidos = 0;
            await _repository.UpdateAsync(user);
        }

        return MapToResponse(user);
    }

    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException("USR-006", "Usuario no encontrado.");
        }

        return MapToResponse(user);
    }

    private UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellido = user.Apellido,
            Email = user.Email,
            FechaRegistro = user.FechaRegistro,
            Activo = user.Activo
        };
    }
}
