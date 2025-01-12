using NLog;
using System.Diagnostics;

namespace DayPlanner.Api.Middleware
{
    /// <summary>
    /// Middleware to add a traceId to the request and response.
    /// </summary>
    internal class TraceIdMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
        public async Task InvokeAsync(HttpContext context)
        {
            // Generate traceId
            var traceId = Activity.Current?.TraceId.ToString();
            context.Items["TraceId"] = traceId;

            // Add traceId to NLog context
            using (ScopeContext.PushProperty("TraceId", traceId))
            {
                // Add traceId to the response header
                context.Response.Headers.Append("Trace-Id", traceId);

                await _next(context);
            }
        }
    }
}
