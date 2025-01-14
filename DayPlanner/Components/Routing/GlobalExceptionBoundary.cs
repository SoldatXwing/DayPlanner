using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Components.Routing;

internal sealed class GlobalExceptionBoundary(ILogger<GlobalExceptionBoundary> logger) : ErrorBoundaryBase
{
    protected override Task OnErrorAsync(Exception exception)
    {
        logger.LogError(exception, "An unhandled exception escalated to the error boundary.");
        return Task.CompletedTask;
    }
}
