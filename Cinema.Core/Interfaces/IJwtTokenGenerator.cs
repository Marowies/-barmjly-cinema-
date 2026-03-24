using Cinema.Core.Entities;

namespace Cinema.Core.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
