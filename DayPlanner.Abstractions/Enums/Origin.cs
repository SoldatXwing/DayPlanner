using System.Text.Json.Serialization;

namespace DayPlanner.Abstractions.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CalendarOrigin
    {
        Unspecified,
        GoogleCalendar,
        WindowsCalendar,
        AiSuggestion,
        Ai
    }
}
