using Cinema.Core.Entities;

namespace Cinema.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<User> AddAsync(User user);
    Task<bool> EmailExistsAsync(string email);
}
