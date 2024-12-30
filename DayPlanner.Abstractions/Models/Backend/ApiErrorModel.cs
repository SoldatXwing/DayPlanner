using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Abstractions.Models.Backend
{
    public record class ApiErrorModel
    {
        public string Message { get; init; } = string.Empty;
        public string Error { get; init; } = string.Empty;
    }
}
