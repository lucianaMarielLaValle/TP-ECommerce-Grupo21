using Users.API.Models;

namespace Users.API.Repositories;

public interface IUserRepository
{
    Task<User> AddAsync(User user);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> UpdateAsync(User user);
}
