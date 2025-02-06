using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Radzen;
using Radzen.Blazor;

namespace DayPlanner.Web.Components.Pages;

[Authorize]
[Route("/dashboard")]
public partial class Dashboard : ComponentBase
{
    [Inject]
    private IStringLocalizer<Dashboard> Localizer { get; set; } = default!;
    [Inject]
    private IAppointmentService AppointmentService { get; set; } = default!;

    private List<Appointment> _appointments = [];
    private (DateTime Start, DateTime End)? _loadedRange;
    private async Task OnLoadData(SchedulerLoadDataEventArgs args)
    {
        if (_loadedRange is null)
        {
            _appointments = await AppointmentService.GetAllAppointmentsInRangeAsync(args.Start, args.End);
            _loadedRange = (args.Start, args.End);
        }
        else
        {
            var (loadedFrom, loadedTo) = _loadedRange.Value;

            // Falls der neue Bereich außerhalb des "linken" kalendarblatts liegt
            if (args.Start < loadedFrom)
            {
                var newAppointmentsLeft = await AppointmentService
                    .GetAllAppointmentsInRangeAsync(args.Start, loadedFrom);

                // Zusammenführen der Listen, ggf. Duplikate entfernen
                _appointments.AddRange(newAppointmentsLeft);
            }

            // Falls der neue Bereich außerhalb des "rechten" kalendarblatts liegt
            if (args.End > loadedTo)
            {
                var newAppointmentsRight = await AppointmentService
                    .GetAllAppointmentsInRangeAsync(loadedTo, args.End);

                _appointments.AddRange(newAppointmentsRight);
            }

            // Gemerkten Bereich auf den neuen ausdehnen
            var newStart = args.Start < loadedFrom ? args.Start : loadedFrom;
            var newEnd = args.End > loadedTo ? args.End : loadedTo;
            _loadedRange = (newStart, newEnd);
        }
    }
    private void OnAppointmentMouseLeave(SchedulerAppointmentMouseEventArgs<Appointment> args)
    {
        TooltipService.Close();
    }
}