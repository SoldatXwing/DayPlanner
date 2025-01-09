﻿namespace DayPlanner.Abstractions.Models.Backend;

public class Appointment
{
    public string Id { get; set; } = string.Empty;

    public required string UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public DateTime CreatedAt { get; set; }
}
