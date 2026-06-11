using Users.API.DTOs;

namespace Users.API.Services;

public interface IUserService
{
    Task<UserResponse> RegisterAsync(RegisterUserRequest request);
    Task<UserResponse> LoginAsync(LoginRequest request);
    Task<UserResponse> GetByIdAsync(Guid id);
}
