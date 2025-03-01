using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Radzen;
using Radzen.Blazor;
using System.Globalization;

namespace DayPlanner.Web.Components.Pages.Account;

[Authorize]
[Route("/dashboard")]
public partial class Dashboard : ComponentBase
{
    #region Injections
    [Inject]
    private IStringLocalizer<Dashboard> Localizer { get; set; } = default!;
    [Inject]
    private IAppointmentService AppointmentService { get; set; } = default!;
    [Inject]
    private DialogService DialogService { get; set; } = default!;
    [Inject]
    private NotificationService NotificationService { get; set; } = default!;
    [Inject]
    private IAiService AiService { get; set; } = default!;
    #endregion

    private string _userAiInput = string.Empty;
    private List<Appointment> _appointments = [];
    private (DateTime Start, DateTime End)? _loadedRange;
    private (DateTime Start, DateTime End)? _currentRange;

    private async Task OnLoadDataAsync(SchedulerLoadDataEventArgs args)
    {
        _currentRange = (args.Start, args.End);
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
    private async Task OnSlotSelectAsync(SchedulerSlotSelectEventArgs args)
    {
        if (args.View.Text != "Year")
        {
            AppointmentRequest data = await DialogService.OpenAsync<AddOrEditAppointment>(Localizer["AddAppointment"],
                new Dictionary<string, object> { { "AppointmentRequest", new AppointmentRequest { Start = args.Start, End = args.End } },
            { "IsEditMode", false } });

            if (data != null)
            {
                Appointment appointment = await AppointmentService.CreateAppointmentAsync(data);
                _appointments = [.. _appointments, .. new[] { appointment }];
                StateHasChanged();
            }
        }
    }
    private void OnAppointmentMouseLeave(SchedulerAppointmentMouseEventArgs<Appointment> args)
    {
        TooltipService.Close();
    }
    private async Task OnAppointmentSelectedAsync(SchedulerAppointmentSelectEventArgs<Appointment> args)
    {
        dynamic result = await DialogService.OpenAsync<AddOrEditAppointment>(Localizer["EditAppointment"],
               new Dictionary<string, object> { { "AppointmentRequest", new AppointmentRequest(args.Data.Origin) {
                   Start = args.Data.Start,
                   End = args.Data.End,
                   Location = args.Data.Location,
                   Summary = args.Data.Summary,
                   Title = args.Data.Title } },
            { "IsEditMode", true } });

        if (result is bool deleteRequested && deleteRequested == false)
        {
            await AppointmentService.DeleteAppointmentAsync(args.Data.Id);
            _appointments = _appointments.Where(a => a.Id != args.Data.Id).ToList();
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Duration = 5000,
                Summary = Localizer["DeleteAppointmentSuccess"]
            });
        }
        if (result is AppointmentRequest data)
        {
            Appointment updatedAppointment = await AppointmentService.UpdateAppointmentAsync(args.Data.Id, data);
            _appointments = _appointments
                .Where(a => a.Id != args.Data.Id)
                .Concat([updatedAppointment])
                .ToList();
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Duration = 5000,
                Summary = Localizer["EditAppointmentSuccess"]
            });
        }
        StateHasChanged();
    }
    private async Task Form_OnSubmitAsync()
    {
        if(string.IsNullOrWhiteSpace(_userAiInput))
            return;

        var utcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        var utcOffsetFormatted = $"UTC: {(utcOffset >= TimeSpan.Zero ? "+" : "-")}{utcOffset.Hours:D2}:{utcOffset.Minutes:D2}";

        var result = await AiService.GetAppointmentSuggestionAsync(_userAiInput, _currentRange!.Value.Start, _currentRange!.Value.End, utcOffsetFormatted, CultureInfo.CurrentUICulture.Name);
    }
}