using DayPlanner.Abstractions.Models.Backend;

namespace DayPlanner.Abstractions.Models.DTO
{
    public class AppointmentRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;

        public string Location = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}