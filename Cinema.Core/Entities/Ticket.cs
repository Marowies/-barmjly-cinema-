using Cinema.Core.Enums;

namespace Cinema.Core.Entities;

public class Ticket
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int ShowTimeId { get; set; }
    public ShowTime ShowTime { get; set; } = null!;
    public int SeatNumber { get; set; }
    public DateTime BookingDate { get; set; }
    public TicketStatus Status { get; set; }
}
