using DayPlanner.Abstractions.Models.Backend;

namespace DayPlanner.Abstractions.Models.DTO
{
    public class AppointmentRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public GeoLocation? Location { get; set; } = null;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}