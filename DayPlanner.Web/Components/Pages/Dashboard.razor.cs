using DayPlanner.Web.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Web.Components.Pages;

[Route("/dashboard")]
[Authorize]
public sealed partial class Dashboard : ComponentBase
{
    [Inject]
    private DefaultAuthenticationService defaultAuthenticationService { get; set; } = default!;
    [Inject]
    private IHttpContextAccessor httpContextAccessor { get; set; } = default!;
    protected async override Task OnInitializedAsync()
    {
        
    }
}
