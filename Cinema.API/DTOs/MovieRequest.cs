namespace Cinema.API.DTOs;

public class MovieRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationInMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Genre { get; set; } = string.Empty;
}
