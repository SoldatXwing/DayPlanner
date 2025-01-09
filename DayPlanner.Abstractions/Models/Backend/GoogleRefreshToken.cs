using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Abstractions.Models.Backend;

public class GoogleRefreshToken
{
    public required string UserId { get; set; }

    public required string RefreshToken { get; set; }
}
