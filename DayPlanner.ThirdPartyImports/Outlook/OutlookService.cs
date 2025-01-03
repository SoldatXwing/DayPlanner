using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.ThirdPartyImports.Outlook
{
    public class OutlookService : IExternalAppointmentService
    {
        public Task<List<Appointment>> GetAppointments(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }
    }
}
