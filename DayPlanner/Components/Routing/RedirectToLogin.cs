using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Components.Routing;

public sealed class RedirectToLogin : ComponentBase
{
    #region Injections
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    #endregion

    protected override void OnInitialized()
    {
        NavigationManager.NavigateToLogin();
    }
}
