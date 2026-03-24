namespace Cinema.Core.Entities;

public class ShowTime
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
    public bool IsExpired { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
