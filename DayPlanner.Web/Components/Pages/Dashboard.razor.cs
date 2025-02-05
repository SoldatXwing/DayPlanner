using DayPlanner.Abstractions.Models.Backend;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace DayPlanner.Web.Components.Pages;

[Route("/dashboard")]
[Authorize]
public partial class Dashboard : ComponentBase
{
    [Inject]
    private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;
    [Inject]
    private IStringLocalizer<Dashboard> Localizer { get; set; } = default!;
    private CultureInfo CultureInfo { get; set; } = default!;

    private List<Appointment> _appointments = [];
    protected async override Task OnInitializedAsync()
    {
        IRequestCultureFeature cultureFeature = HttpContextAccessor.HttpContext!.Features.GetRequiredFeature<IRequestCultureFeature>();
        CultureInfo = cultureFeature.RequestCulture.UICulture;
        _appointments =
        [
            new Appointment{UserId = "s", Title = "Meeting with John Doe", Start = DateTime.Now, End = DateTime.Now.AddHours(1) },
            new Appointment{UserId = "s", Title = "Meeting with Brohn Doe", Start = DateTime.Now, End = DateTime.Now.AddHours(2) },
            new Appointment{UserId = "s", Title = "Meeting with Klone Doe", Start = DateTime.Now, End = DateTime.Now.AddHours(3) },
        ];
    }
    protected override void OnAfterRender(bool firstRender)
    {
        //Keep the red current time line active
        if (firstRender)
        {
            var timer = new Timer(_ => InvokeAsync(StateHasChanged));
            timer.Change(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }
    }


}
