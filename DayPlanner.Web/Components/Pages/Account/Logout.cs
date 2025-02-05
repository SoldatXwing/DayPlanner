using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace DayPlanner.Web.Components.Pages.Account;

[AllowAnonymous]
[Route("/account/logout")]
[ExcludeFromInteractiveRouting]
public sealed class Logout : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [CascadingParameter]
    private HttpContext? HttpContext { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (HttpContext is null)
        {
            NavigationManager.Refresh(forceReload: true);
            return;
        }

        await HttpContext.SignOutAsync();
        NavigationManager.NavigateToHome();
    }
}
