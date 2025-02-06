﻿using DayPlanner.Abstractions.Models.Backend;
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
            StateHasChanged();
        }
        else
        {
            var (loadedFrom, loadedTo) = _loadedRange.Value;

            //User navigated in the schedular to the "left"
            if (args.Start < loadedFrom)
            {
                var newAppointmentsLeft = await AppointmentService
                    .GetAllAppointmentsInRangeAsync(args.Start, loadedFrom);

                _appointments = [.. _appointments, .. newAppointmentsLeft];
            }
            //User navigated in the schedular to the "right"
            if (args.End > loadedTo)
            {
                var newAppointmentsRight = await AppointmentService
                    .GetAllAppointmentsInRangeAsync(loadedTo, args.End);

                _appointments = [.. _appointments, .. newAppointmentsRight]; // its necessary to create a "new" list, to notfiy that the list changed.
            }

            _loadedRange = (new DateTime(Math.Min(args.Start.Ticks, loadedFrom.Ticks)),
                            new DateTime(Math.Max(args.End.Ticks, loadedTo.Ticks)));
            StateHasChanged();
        }
    }
    private void OnAppointmentMouseLeave(SchedulerAppointmentMouseEventArgs<Appointment> args)
    {
        TooltipService.Close();
    }
}