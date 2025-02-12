using System.ComponentModel.DataAnnotations;

namespace DayPlanner.Abstractions.Models.DTO;

public class RegisterUserRequest
{
    public required string Password { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; } = null;
    public string? PhotoUrl { get; set; } = null;

}
