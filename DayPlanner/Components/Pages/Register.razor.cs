﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Components.Pages;

[AllowAnonymous]
[Route("/register")]
public sealed partial class Register : ComponentBase
{
}
