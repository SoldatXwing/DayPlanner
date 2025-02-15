namespace DayPlanner.Abstractions.Models.DTO;

public class UpdateUserRequest
{
    public string Uid { get; set; } = string.Empty;
    public string? Password { get; set; } = string.Empty;
    public string? DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    //public byte[]? PhotoBytes { get; set; } = null;
}