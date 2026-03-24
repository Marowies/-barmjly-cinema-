using Cinema.API.DTOs;
using Cinema.Core.Entities;
using Cinema.Core.Enums;
using Cinema.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthController(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            return BadRequest(new { message = "Email already exists" });
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            return BadRequest(new { message = "Invalid role" });
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FullName = request.FullName,
            Role = role
        };

        await _userRepository.AddAsync(user);

        var token = _jwtTokenGenerator.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString()
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString()
        });
    }
}
