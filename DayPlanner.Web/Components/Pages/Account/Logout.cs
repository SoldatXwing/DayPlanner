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
    private HttpContext HttpContext { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await HttpContext.SignOutAsync();

#if DEBUG
        try
        {
            NavigationManager.NavigateToHome();
        }
        catch (NavigationException)
        {
            throw;     // This prevents the debugger from breaking at this point.
        }
#else
        NavigationManager.NavigateToHome();
#endif
    }
}
