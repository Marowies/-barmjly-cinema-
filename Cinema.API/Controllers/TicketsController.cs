using Cinema.API.DTOs;
using Cinema.Core.Entities;
using Cinema.Core.Enums;
using Cinema.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cinema.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "User")]
public class TicketsController : ControllerBase
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IShowTimeRepository _showTimeRepository;

    public TicketsController(ITicketRepository ticketRepository, IShowTimeRepository showTimeRepository)
    {
        _ticketRepository = ticketRepository;
        _showTimeRepository = showTimeRepository;
    }

    [HttpGet("my-tickets")]
    public async Task<IActionResult> GetMyTickets()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var tickets = await _ticketRepository.GetByUserIdAsync(userId);
        return Ok(tickets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);
        if (ticket == null)
            return NotFound(new { message = "Ticket not found" });

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (ticket.UserId != userId)
            return Forbid();

        return Ok(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> BookTicket([FromBody] TicketRequest request)
    {
        var showTime = await _showTimeRepository.GetByIdAsync(request.ShowTimeId);
        if (showTime == null)
            return NotFound(new { message = "ShowTime not found" });

        if (showTime.IsExpired)
            return BadRequest(new { message = "ShowTime has expired" });

        if (showTime.AvailableSeats <= 0)
            return BadRequest(new { message = "No available seats" });

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var ticket = new Ticket
        {
            UserId = userId,
            ShowTimeId = request.ShowTimeId,
            SeatNumber = request.SeatNumber,
            BookingDate = DateTime.Now,
            Status = TicketStatus.Active
        };

        await _ticketRepository.AddAsync(ticket);

        showTime.AvailableSeats--;
        await _showTimeRepository.UpdateAsync(showTime);

        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelTicket(int id)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);
        if (ticket == null)
            return NotFound(new { message = "Ticket not found" });

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (ticket.UserId != userId)
            return Forbid();

        if (ticket.Status == TicketStatus.Cancelled)
            return BadRequest(new { message = "Ticket already cancelled" });

        ticket.Status = TicketStatus.Cancelled;
        await _ticketRepository.UpdateAsync(ticket);

        var showTime = await _showTimeRepository.GetByIdAsync(ticket.ShowTimeId);
        if (showTime != null)
        {
            showTime.AvailableSeats++;
            await _showTimeRepository.UpdateAsync(showTime);
        }

        return Ok(new { message = "Ticket cancelled successfully" });
    }
}
