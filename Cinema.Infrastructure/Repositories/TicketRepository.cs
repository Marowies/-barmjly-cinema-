using Cinema.Core.Entities;
using Cinema.Core.Interfaces;
using Cinema.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly ApplicationDbContext _context;

    public TicketRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Ticket>> GetAllAsync()
    {
        return await _context.Tickets
            .Include(t => t.User)
            .Include(t => t.ShowTime)
                .ThenInclude(s => s.Movie)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByUserIdAsync(int userId)
    {
        return await _context.Tickets
            .Include(t => t.ShowTime)
                .ThenInclude(s => s.Movie)
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task<Ticket?> GetByIdAsync(int id)
    {
        return await _context.Tickets
            .Include(t => t.User)
            .Include(t => t.ShowTime)
                .ThenInclude(s => s.Movie)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Ticket> AddAsync(Ticket ticket)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket != null)
        {
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
        }
    }
}
