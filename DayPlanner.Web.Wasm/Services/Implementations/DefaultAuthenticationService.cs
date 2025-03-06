using Microsoft.AspNetCore.Components.Authorization;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Wasm.Models;
using DayPlanner.Web.Wasm.Refit;
using Refit;
using System.Net;
using System.Security.Claims;
using DayPlanner.Web.Wasm.Extensions;
using Blazored.LocalStorage;

namespace DayPlanner.Web.Wasm.Services.Implementations
{
    internal class DefaultAuthenticationService(IDayPlannerAccountApi api, ILocalStorageService localStorageService) : AuthenticationStateProvider, IAuthenticationService
    {
        private UserSession? _currentUserSession;

        public async Task<UserSession?> LoginAsync(UserRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            TokenResponse tokenData;
            User user;
            try
            {
                tokenData = await api.LoginAsync(request);
                user = await api.GetCurrentUserAsync(tokenData.Token);
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var userSession = new UserSession()
            {
                Uid = user.Uid,
                Token = tokenData.Token,
                RefreshToken = tokenData.RefreshToken,
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                PhotoUrl = user.PhotoUrl,
                LastSignInTimestamp = user.LastSignInTimestamp,
                EmailVerified = user.EmailVerified
            };

            await SaveUserSession(userSession);
            NotifyAuthenticationStateChanged();
            return userSession;
        }

        public async Task<(UserSession? user, ApiErrorModel? error)> RegisterAsync(RegisterUserRequest request)
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

            var userSession = new UserSession()
            {
                Uid = user.Uid,
                Token = tokenData.Token,
                RefreshToken = tokenData.RefreshToken,
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                PhotoUrl = user.PhotoUrl,
                LastSignInTimestamp = user.LastSignInTimestamp,
                EmailVerified = user.EmailVerified
            };

            await SaveUserSession(userSession);
            NotifyAuthenticationStateChanged();
            return (userSession, null);
        }

        public async Task<string> GetGoogleAuthUrlAsync() => await api.GetGoogleAuthUrl("web");

        public async Task<(UserSession? user, ApiErrorModel? error)> LoginViaGoogleAsync(string token)
        {
            try
            {
                var userId = await api.ValidateTokenAsync(token);
                if (string.IsNullOrEmpty(userId))
                    return (null, new ApiErrorModel { Message = "Invalid Google token" });

                var user = await api.GetCurrentUserAsync(token);
                var userSession = new UserSession()
                {
                    Uid = user.Uid,
                    Token = token,
                    RefreshToken = token,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    PhotoUrl = user.PhotoUrl,
                    LastSignInTimestamp = user.LastSignInTimestamp,
                    EmailVerified = user.EmailVerified
                };

                await SaveUserSession(userSession);
                NotifyAuthenticationStateChanged();
                return (userSession, null);
            }
            catch (ApiException ex)
            {
                return (null, new ApiErrorModel { Message = ex.Content! });
            }
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userSessionResult = await localStorageService.GetItemAsync<UserSession>("userSession");
                if (userSessionResult is not null)
                {
                    _currentUserSession = userSessionResult;
                    return new AuthenticationState(new ClaimsPrincipal(_currentUserSession.ToClaimsPrincipial()));
                }
            }
            catch
            {
            }

            _currentUserSession = null;
            return new AuthenticationState(new ClaimsPrincipal());
        }

        public async Task SignOutAsync()
        {
            await localStorageService.RemoveItemAsync("userSession");
            NotifyAuthenticationStateChanged();
        }

        private async Task SaveUserSession(UserSession userSession)
        {
            _currentUserSession = userSession;
            await localStorageService.SetItemAsync("userSession", userSession);
        }

        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}