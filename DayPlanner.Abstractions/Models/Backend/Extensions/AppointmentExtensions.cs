namespace DayPlanner.Abstractions.Models.Backend.Extensions
{
    public static class AppointmentExtensions
    {
        public static Dictionary<string, object> ToDictionary(this Appointment appointment) => new()
        {

                { "id", appointment.Id },

                { "userId", appointment.UserId },

                { "title", appointment.Title },

                { "summary", appointment.Summary },

                { "startDate", appointment.Start },

                { "endDate", appointment.End },
                { "createdAt", appointment.CreatedAt },

                { "location", appointment.Location }
            };
    }
}
