using Cinema.API.DTOs;
using Cinema.Core.Entities;
using Cinema.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;

    public MoviesController(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var movies = await _movieRepository.GetAllAsync();
        return Ok(movies);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        if (movie == null)
            return NotFound(new { message = "Movie not found" });
        
        return Ok(movie);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MovieRequest request)
    {
        var movie = new Movie
        {
            Title = request.Title,
            Description = request.Description,
            DurationInMinutes = request.DurationInMinutes,
            ReleaseDate = request.ReleaseDate,
            Genre = request.Genre
        };

        await _movieRepository.AddAsync(movie);
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MovieRequest request)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        if (movie == null)
            return NotFound(new { message = "Movie not found" });

        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.DurationInMinutes = request.DurationInMinutes;
        movie.ReleaseDate = request.ReleaseDate;
        movie.Genre = request.Genre;

        await _movieRepository.UpdateAsync(movie);
        return Ok(movie);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        if (movie == null)
            return NotFound(new { message = "Movie not found" });

        await _movieRepository.DeleteAsync(id);
        return NoContent();
    }
}
