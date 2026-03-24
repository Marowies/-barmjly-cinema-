using Cinema.API.DTOs;
using Cinema.Core.Entities;
using Cinema.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ShowTimesController : ControllerBase
{
    private readonly IShowTimeRepository _showTimeRepository;
    private readonly IMovieRepository _movieRepository;

    public ShowTimesController(IShowTimeRepository showTimeRepository, IMovieRepository movieRepository)
    {
        _showTimeRepository = showTimeRepository;
        _movieRepository = movieRepository;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var showTimes = await _showTimeRepository.GetAllAsync();
        return Ok(showTimes);
    }

    [AllowAnonymous]
    [HttpGet("movie/{movieId}")]
    public async Task<IActionResult> GetByMovieId(int movieId)
    {
        var showTimes = await _showTimeRepository.GetByMovieIdAsync(movieId);
        return Ok(showTimes);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var showTime = await _showTimeRepository.GetByIdAsync(id);
        if (showTime == null)
            return NotFound(new { message = "ShowTime not found" });
        
        return Ok(showTime);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ShowTimeRequest request)
    {
        var movie = await _movieRepository.GetByIdAsync(request.MovieId);
        if (movie == null)
            return NotFound(new { message = "Movie not found" });

        var showTime = new ShowTime
        {
            MovieId = request.MovieId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Price = request.Price,
            AvailableSeats = request.AvailableSeats,
            IsExpired = false
        };

        await _showTimeRepository.AddAsync(showTime);
        return CreatedAtAction(nameof(GetById), new { id = showTime.Id }, showTime);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ShowTimeRequest request)
    {
        var showTime = await _showTimeRepository.GetByIdAsync(id);
        if (showTime == null)
            return NotFound(new { message = "ShowTime not found" });

        showTime.MovieId = request.MovieId;
        showTime.StartTime = request.StartTime;
        showTime.EndTime = request.EndTime;
        showTime.Price = request.Price;
        showTime.AvailableSeats = request.AvailableSeats;

        await _showTimeRepository.UpdateAsync(showTime);
        return Ok(showTime);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var showTime = await _showTimeRepository.GetByIdAsync(id);
        if (showTime == null)
            return NotFound(new { message = "ShowTime not found" });

        await _showTimeRepository.DeleteAsync(id);
        return NoContent();
    }
}
