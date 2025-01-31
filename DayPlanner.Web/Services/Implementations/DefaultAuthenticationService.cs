using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Models;
using DayPlanner.Web.Refit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Newtonsoft.Json;
using Refit;
using System.Net;
using System.Security.Claims;
using DayPlanner.Web.Extensions;

namespace DayPlanner.Web.Services.Implementations
{
    internal class DefaultAuthenticationService(IDayPlannerAccountApi api, ProtectedLocalStorage securedStorage, ILogger<DefaultAuthenticationService> logger) : AuthenticationStateProvider, IAuthenticationService
    {
        private readonly IDayPlannerAccountApi _api = api;
        private readonly ProtectedLocalStorage _localStorage = securedStorage;
        private readonly ILogger<DefaultAuthenticationService> _logger = logger;
        private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

        private const string AuthTokenKey = "authToken";
        private const string UserKey = "user";

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorage.GetAsync<string>(AuthTokenKey);

                var user = await _localStorage.GetAsync<User>(UserKey);

                if (string.IsNullOrWhiteSpace(token.Value) || user.Value == null)
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                try
                {
                    _currentUser = new ClaimsPrincipal(user.Value.ToClaimsPrincipial());
                }
                catch
                {
                    _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                    await _localStorage.DeleteAsync(AuthTokenKey);
                    await _localStorage.DeleteAsync(UserKey);
                }

                return new AuthenticationState(_currentUser);
            }
            catch
            {
                return new(new());
            }
           
        }
        public async Task<User?> LoginAsync(UserRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            TokenResponse tokenData;
            try
            {
                tokenData = await api.LoginAsync(request);
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var user = await _api.GetCurrentUserAsync(tokenData.Token);
            await SaveUserAndTokenAsync(user, tokenData.Token);

            _currentUser = new ClaimsPrincipal(user.ToClaimsPrincipial());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            return user;
        }
        

        public async Task LogoutAsync()
        {
            await _localStorage.DeleteAsync(AuthTokenKey);
            await _localStorage.DeleteAsync(UserKey);
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task<(User? user, ApiErrorModel? error)> RegisterAsync(RegisterUserRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            User user;
            TokenResponse tokenData;
            try
            {
                user = await api.RegisterUserAsync(request);
                tokenData = await api.LoginAsync(new() { Email = request.Email!, Password = request.Password });
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                ApiErrorModel? errorModel = await ex.GetContentAsAsync<ApiErrorModel>();
                return (null, errorModel);
            }

            await SaveUserAndTokenAsync(user, tokenData.Token);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            return (user, null);
        }

        public async Task<string> GetGoogleAuthUrlAsync()
        {
            return await _api.GetGoogleAuthUrl();
        }

        public async Task<(User? user, ApiErrorModel? error)> LoginViaGoogleAsync(string token)
        {
            //try
            //{
            //    var userId = await _api.ValidateTokenAsync(token);
            //    if (string.IsNullOrEmpty(userId))
            //        return (null, new ApiErrorModel { Message = "Invalid Google token" });

            //    var user = await _api.GetCurrentUserAsync(token);
            //    await SaveUserAndTokenAsync(user, token);

            //    _currentUser = new ClaimsPrincipal(CreateIdentity(user, token));
            //    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            //    return (user, null);
            //}
            //catch (ApiException ex)
            //{
            //    return (null, new ApiErrorModel { Message = ex.Content });
            //}
            return (null, null);
        }

        private async Task SaveUserAndTokenAsync(User user, string token)
        {
            await _localStorage.SetAsync(AuthTokenKey, token);
            await _localStorage.SetAsync(UserKey, user);
        }
       
    }
}
