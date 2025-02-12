namespace DayPlanner.Abstractions.Models.DTO;

public class UpdateUserRequest
{
    public string? Password { get; set; } = null;
    public string? DisplayName { get; set; } = null;
    public string? Email { get; set; } = null;
    public string? PhoneNumber { get; set; } = null;
    public string? PhotoUrl { get; set; } = null;
}