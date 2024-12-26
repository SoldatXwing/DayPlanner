namespace DayPlanner.Abstractions.Models.Backend.Extensions
{
    public static class AppointmentExtensions
    {
        public static Dictionary<string, object> ToDictionary(this Appointment appointment) => new()
        {

                { "title", appointment.Title },

                { "summary", appointment.Summary },

                { "startDate", appointment.Start },

                { "endDate", appointment.End },
                { "location", appointment.Location }
            };
    }
}
