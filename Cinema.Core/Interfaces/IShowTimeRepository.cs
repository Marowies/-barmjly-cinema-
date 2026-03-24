using Cinema.Core.Entities;

namespace Cinema.Core.Interfaces;

public interface IShowTimeRepository
{
    Task<IEnumerable<ShowTime>> GetAllAsync();
    Task<IEnumerable<ShowTime>> GetByMovieIdAsync(int movieId);
    Task<ShowTime?> GetByIdAsync(int id);
    Task<ShowTime> AddAsync(ShowTime showTime);
    Task UpdateAsync(ShowTime showTime);
    Task DeleteAsync(int id);
    Task ExpireOldShowTimesAsync();
}
