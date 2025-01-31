using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Web.Components.Pages
{
    [Route("/")]
    [AllowAnonymous]
    public partial class Home : ComponentBase
    {
    }
}
