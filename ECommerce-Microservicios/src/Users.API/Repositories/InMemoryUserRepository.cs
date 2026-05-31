using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public Task<User> AddAsync(User user)
    {
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        foreach (var user in _users)
        {
            if (user.Id == id)
            {
                return Task.FromResult<User?>(user);
            }
        }

        return Task.FromResult<User?>(null);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        foreach (var user in _users)
        {
            if (string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<User?>(user);
            }
        }

        return Task.FromResult<User?>(null);
    }

    public Task<User> UpdateAsync(User user)
    {
        foreach (var existing in _users)
        {
            if (existing.Id == user.Id)
            {
                _users.Remove(existing);
                _users.Add(user);
                return Task.FromResult(user);
            }
        }

        throw new NotFoundException("USR-006", "Usuario no encontrado para actualizar.");
    }
}
