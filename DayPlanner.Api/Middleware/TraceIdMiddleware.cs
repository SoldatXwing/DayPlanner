using NLog;
using System.Diagnostics;

namespace DayPlanner.Api.Middleware
{
    /// <summary>
    /// Middleware to add a traceId to the request and response.
    /// </summary>
    public class TraceIdMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
        /// <summary>
        /// Middleware to add a TraceId to the current HTTP request and response for tracking purposes.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> representing the current HTTP request and response.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the completion of request processing by this middleware.
        /// </returns>
        /// <remarks>
        /// - Generates a unique TraceId for the current request if available via <see cref="Activity.Current"/>.
        /// - Stores the TraceId in the <see cref="HttpContext.Items"/> dictionary under the key "TraceId".
        /// - Adds the TraceId to the response headers as "Trace-Id".
        /// - Pushes the TraceId into the NLog <see cref="ScopeContext"/> for logging.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
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
