namespace DayPlanner.Abstractions.Models.DTO;

public class UpdateUserRequest
{
    public string Uid { get; set; } = default!;
    public string? Password { get; set; } = null;
    public string? DisplayName { get; set; } = null;
    public string? Email { get; set; } = null;
    //public byte[]? PhotoBytes { get; set; } = null;
}