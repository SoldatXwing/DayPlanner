using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Abstractions.Models.DTO
{
    public class AppointmentSuggestion
    {
        public required string PromptMessage { get; set; }
        public required string Title { get; set; }
        public string Description { get; set; } = default!;
        public required DateTime Start { get; set; } 
        public required DateTime End { get; set; }
    }
}
