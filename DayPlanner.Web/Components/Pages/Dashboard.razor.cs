using DayPlanner.Abstractions.Models.Backend;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Radzen.Blazor;

namespace DayPlanner.Web.Components.Pages;

[Authorize]
[Route("/dashboard")]
public partial class Dashboard : ComponentBase
{
    [Inject]
    private IStringLocalizer<Dashboard> Localizer { get; set; } = default!;

    private List<Appointment> _appointments = [
        new Appointment{UserId = "s", Title = "Meeting with John Doe", Start = DateTime.Now, End = DateTime.Now.AddHours(1) },
        new Appointment{UserId = "s", Title = "Meeting with Brohn Doe", Start = DateTime.Now, End = DateTime.Now.AddHours(2) },
        new Appointment{UserId = "s", Title = "Meeting with Klone Doe", Start = DateTime.Now, End = DateTime.Now.AddHours(3) },
    ];

    private void OnAppointmentMouseLeave(SchedulerAppointmentMouseEventArgs<Appointment> args)
    {
        TooltipService.Close();
    }
}