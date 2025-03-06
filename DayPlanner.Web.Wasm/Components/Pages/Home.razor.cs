using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Web.Wasm.Components.Pages
{
   
    [Route("/")]
    [AllowAnonymous]
    public partial class Home : ComponentBase
    {
        #region Injections
        [Inject]
        private IStringLocalizer<Home> Localizer { get; set; } = default!;
        [Inject]
        private NavigationManager Navigation { get; set; } = default!;
        #endregion

    }
}
