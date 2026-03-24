using Cinema.Core.Entities;

namespace Cinema.Core.Interfaces;

public interface ITicketRepository
{
    Task<IEnumerable<Ticket>> GetAllAsync();
    Task<IEnumerable<Ticket>> GetByUserIdAsync(int userId);
    Task<Ticket?> GetByIdAsync(int id);
    Task<Ticket> AddAsync(Ticket ticket);
    Task UpdateAsync(Ticket ticket);
    Task DeleteAsync(int id);
}
