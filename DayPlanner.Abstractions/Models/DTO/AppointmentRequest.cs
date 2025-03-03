using DayPlanner.Abstractions.Enums;
using DayPlanner.Abstractions.Models.Backend;

namespace DayPlanner.Abstractions.Models.DTO
{
    public class AppointmentRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public CalendarOrigin Origin { get; set; } = CalendarOrigin.Unspecified;
        public AppointmentRequest() { }
        /// <summary>
        /// This ctor is needed for properly firebase serialization
        /// </summary>
        /// <param name="calendarOrigin">The calendar origin</param>
        public AppointmentRequest(CalendarOrigin calendarOrigin) => Origin = calendarOrigin;
    }
}