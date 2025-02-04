using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;

namespace DayPlanner.Web.Middleware
{
    /// <summary>
    /// Middleware to handle login requests from Blazor Server
    /// </summary>
    /// <remarks>
    /// <see href="https://github.com/dotnet/aspnetcore/issues/13601#issuecomment-679870698">Based on this solution on github</see>
    /// </remarks>
    internal class BlazorCookieLoginMiddleware
    {
        private readonly RequestDelegate _next;
        public static IDictionary<Guid, UserRequest> Logins { get; private set; }
            = new ConcurrentDictionary<Guid, UserRequest>();

        public BlazorCookieLoginMiddleware(RequestDelegate next) => _next = next;
        public async Task Invoke(HttpContext context, Services.IAuthenticationService authenticationService)
        {
            switch (context.Request.Path.Value)
            {
                case "/account/login" when context.Request.Query.ContainsKey("key"):
                    {
                        var key = Guid.Parse(context.Request.Query["key"]!);
                        var info = Logins[key];

                        var user = await authenticationService.LoginAsync(info);
                        if (user is not null)
                        {
                            Logins.Remove(key);
                            await context.SignInAsync(user.ToClaimsPrincipial());
                            context.Response.Redirect("/dashboard");
                            return;
                        }
                        else
                        {
                            context.Response.Redirect("/account/login?showError=true");
                            return;
                        }
                    }
                case "/account/logout":
                    {
                        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
                        context.Response.Redirect("/");
                        return;
                    }
                case "/account/google/login"
                    when context.Request.Query.ContainsKey("token")
                      && context.Request.Query.ContainsKey("refreshToken"):
                    {
                        string token = context.Request.Query["token"]!;

                        var (user, error) = await authenticationService.LoginViaGoogleAsync(token);
                        if (user is null)
                        {
                            context.Response.Redirect("/");
                            return;
                        }
                        await context.SignInAsync(user.ToClaimsPrincipial());
                        context.Response.Redirect("/dashboard");
                        return;
                    }
                default:
                    {
                        await _next.Invoke(context);
                        return;
                    }
            }

        }
    }
}
