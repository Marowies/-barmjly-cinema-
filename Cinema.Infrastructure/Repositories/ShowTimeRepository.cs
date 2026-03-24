using Cinema.Core.Entities;
using Cinema.Core.Interfaces;
using Cinema.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Repositories;

public class ShowTimeRepository : IShowTimeRepository
{
    private readonly ApplicationDbContext _context;

    public ShowTimeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ShowTime>> GetAllAsync()
    {
        return await _context.ShowTimes
            .Include(s => s.Movie)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShowTime>> GetByMovieIdAsync(int movieId)
    {
        return await _context.ShowTimes
            .Where(s => s.MovieId == movieId)
            .ToListAsync();
    }

    public async Task<ShowTime?> GetByIdAsync(int id)
    {
        return await _context.ShowTimes
            .Include(s => s.Movie)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ShowTime> AddAsync(ShowTime showTime)
    {
        _context.ShowTimes.Add(showTime);
        await _context.SaveChangesAsync();
        return showTime;
    }

    public async Task UpdateAsync(ShowTime showTime)
    {
        _context.ShowTimes.Update(showTime);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var showTime = await _context.ShowTimes.FindAsync(id);
        if (showTime != null)
        {
            _context.ShowTimes.Remove(showTime);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ExpireOldShowTimesAsync()
    {
        var expiredShowTimes = await _context.ShowTimes
            .Where(s => s.EndTime < DateTime.Now && !s.IsExpired)
            .ToListAsync();

        foreach (var showTime in expiredShowTimes)
        {
            showTime.IsExpired = true;
        }

        await _context.SaveChangesAsync();
    }
}
