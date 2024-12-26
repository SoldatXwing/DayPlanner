using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Abstractions.Models.Backend
{
    public class Appointment
    {
        public string Id { get; set; } = string.Empty;
        public required string UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public GeoLocation? Location { get; set; } = null;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

    }

    public class GeoLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
