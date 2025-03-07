using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Wasm.Services;
using DayPlanner.Web.Wasm.Extensions;
using DayPlanner.Web.Wasm.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Radzen;
using Microsoft.AspNetCore.Http;
using DayPlanner.Web.Wasm.Services.Implementations;

namespace DayPlanner.Web.Wasm.Components.Pages.Account
{
    [Route("/account/settings")]
    public partial class Settings : ComponentBase
    {
        #region Injections
        [Inject]
        private IStringLocalizer<Settings> Localizer { get; set; } = default!;
        [Inject]
        private IUserService UserService { get; set; } = default!;
        [Inject]
        private NotificationService NotificationService { get; set; } = default!;
        [Inject]
        private NavigationManager Navigation { get; set; } = default!;
        [Inject]
        private IGoogleCalendarService GoogleCalendarService { get; set; } = default!;
        [Inject]
        private DialogService DialogService { get; set; } = default!;
        [Inject]
        private DefaultAuthenticationService AuthenticationService { get; set; } = default!;
        #endregion
        #region Parameters
        [SupplyParameterFromQuery(Name = "key")]
        private string? ModelKey { get; set; }
        [SupplyParameterFromQuery(Name = "success")]
        private bool? Success { get; set; }
        #endregion
        #region Cascading Parameters

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationState { get; set; } = default!;
        [CascadingParameter]
        private HttpContext? HttpContext { get; set; } = default!;
        #endregion
        #region Google properties
        private bool isGoogleConnected = false;
        private string googleConnectionStatus = string.Empty;
        #endregion

        private UserSession? UserSession;
        private UpdateUserRequest? UserRequest;

        private bool editMode = false;
        private void CancelEdit()
        {
            editMode = false;

            UserRequest!.Email = UserSession!.Email!;
            UserRequest!.DisplayName = UserSession!.DisplayName!;

            StateHasChanged();
        }
        protected override async Task OnInitializedAsync()
        {
            AuthenticationState authState = await AuthenticationState;
            UserSession = authState.User.Identity?.IsAuthenticated == true
                ? authState.User.ToUserSession()
                : null;

            UserRequest = new UpdateUserRequest
            {
                Email = UserSession!.Email!,
                DisplayName = UserSession!.DisplayName!
            };

            isGoogleConnected = await GoogleCalendarService.IsConnected();

            if (Success is not null && Success.Value)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Duration = 5000,
                    Summary = Localizer["UpdateUserSuccess"]
                });
            }
        }
        private async Task SaveUserSettings()
        {
            if (UserRequest is null)
            {
                return;
            }

            if (UserRequest.Email == UserSession!.Email
                && UserRequest.DisplayName == UserSession.DisplayName) //No changes are made
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Duration = 5000,
                    Summary = Localizer["NoChangesAreMadeMessage"]
                });
                return;
            }
            (User? user, ApiErrorModel? error) = await UserService.UpdateUserAsync(UserRequest);
            if (error is not null)
            {
                return; //TODO: Search for possible errors 
            }
            await AuthenticationService.UpdateUserSessionAsync(new UserSession()
            {
                Uid = user!.Uid,
                Token = UserSession!.Token!,
                RefreshToken = UserSession!.RefreshToken!,
                DisplayName = user!.DisplayName,
                Email = user!.Email,
                PhoneNumber = user!.PhoneNumber,
                PhotoUrl = user!.PhotoUrl,
                LastSignInTimestamp = user!.LastSignInTimestamp,
                EmailVerified = user!.EmailVerified
            });

        }
        private async Task ConnectGoogleCalendar()
        {
            var url = await GoogleCalendarService.GetAuthUrlAsync();
            Navigation.NavigateTo(url.Trim('"'), true);
        }
        private async Task DisconnectGoogleCalendar(bool deleteImportedAppointments)
        {
            await GoogleCalendarService.DisconnectAsync(deleteImportedAppointments);
            Navigation.Refresh(forceReload: true);
        }
        private string GetUserAvatar()
        {
            if (UserSession is null || string.IsNullOrWhiteSpace(UserSession.DisplayName))
                return "U"; // Standard-Avatar, wenn kein Benutzername vorhanden ist.
            if (!string.IsNullOrEmpty(UserSession.PhotoUrl))
            {
                return UserSession.PhotoUrl;
            }
            var parts = UserSession.DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = string.Concat(parts.Select(part => part[0]).Take(2)); 
            return initials.ToUpper(); 
        }
    }
}
