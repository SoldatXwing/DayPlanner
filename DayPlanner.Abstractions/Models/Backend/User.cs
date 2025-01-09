using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Abstractions.Models.Backend;

public class User
{
    /// <summary>
    /// Gets the user ID of this user.
    /// </summary>
    public required string Uid { get; set; }

    /// <summary>
    /// Gets the user's display name, if available. Otherwise null.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets the user's email address, if available. Otherwise null.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets the user's phone number, if available. Otherwise null.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets the user's photo URL, if available. Otherwise null.
    /// </summary>
    public string? PhotoUrl { get; set; }
}