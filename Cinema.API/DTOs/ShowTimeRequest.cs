namespace Cinema.API.DTOs;

public class ShowTimeRequest
{
    public int MovieId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
}
