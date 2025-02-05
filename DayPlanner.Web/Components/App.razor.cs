using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace DayPlanner.Web.Components;

public sealed partial class App : ComponentBase
{
    [Inject]
    private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

    private CultureInfo _uiCulture = default!;

    protected override void OnInitialized()    {
        IRequestCultureFeature cultureFeature = HttpContextAccessor.HttpContext!.Features.GetRequiredFeature<IRequestCultureFeature>();
        _uiCulture = cultureFeature.RequestCulture.UICulture;     // It's ok to retrieve the culture only once per connection because its only updated when the connection is established.
    }
}
