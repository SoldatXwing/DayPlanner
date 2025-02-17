using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Extensions;
using DayPlanner.Web.Models;
using DayPlanner.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Radzen;

namespace DayPlanner.Web.Components.Pages
{
    [Route("/settings")]
    public partial class Settings : ComponentBase
    {
        #region Injections
        [Inject]
        private AuthenticationStateProvider StateProvider { get; set; } = default!;
        [Inject]
        private IStringLocalizer<Settings> Localizer { get; set; } = default!;
        [Inject]
        private DayPlanner.Web.Services.IUserService UserService { get; set; } = default!;
        [Inject]
        private NotificationService NotificationService { get; set; } = default!;
        [Inject]
        private IMemoryCache Cache { get; set; } = default!;
        [Inject]
        private NavigationManager Navigation { get; set; } = default!;
        [Inject]
        private IGoogleCalendarService GoogleCalendarService { get; set; } = default!;
        [Inject]
        private DialogService DialogService { get; set; } = default!;
        #endregion

        [SupplyParameterFromQuery(Name = "key")]
        private string? ModelKey { get; set; }
        [SupplyParameterFromQuery(Name = "success")]
        private bool? Success { get; set; }
        [CascadingParameter]
        private HttpContext? HttpContext { get; set; } = default!;

        private UserSession? UserSession;
        private UpdateUserRequest? UserRequest;

        private Guid formKey = Guid.NewGuid(); //This key is needed, for reloading the validations error, if the user cancels the form

        #region Google properties
        private bool isGoogleConnected = false;
        private string googleConnectionStatus = string.Empty;
        #endregion

        private bool editMode = false;

        private void CancelEdit()
        {
            editMode = false;

            UserRequest!.Email = UserSession!.Email!;
            UserRequest!.DisplayName = UserSession!.DisplayName!;

            formKey = Guid.NewGuid();
            StateHasChanged();
        }
        protected override async Task OnInitializedAsync()
        {
            if (ModelKey is not null)
            {
                if (HttpContext is null)
                {
                    Navigation.Refresh(true);
                    return;
                }
                if (Cache.TryGetValue(ModelKey, out UserSession? cachedUser))
                {
                    Cache.Remove(ModelKey);

                    if (cachedUser is not null)
                    {
                        await HttpContext.SignInAsync(cachedUser.ToClaimsPrincipial());
                    }
                    Navigation.NavigateToSettings(success: true, forceLoad: true);
                    return;
                }
            }

            var authState = await StateProvider.GetAuthenticationStateAsync();
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
            string key = Guid.NewGuid().ToString();

            using ICacheEntry _ = Cache.CreateEntry(key)
                .SetValue(new UserSession()
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
                })
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

            Navigation.NavigateToSettings(key: key, forceLoad: true);

        }


        private async Task ConnectGoogleCalendar()
        {
            var url = await GoogleCalendarService.GetAuthUrlAsync();
            Navigation.NavigateTo(url.Trim('"'), true);
        }

        private async Task DisconnectGoogleCalendar(bool deleteImportedAppointments)
        {
            await GoogleCalendarService.DisconnectAsync(deleteImportedAppointments);
            Navigation.Refresh(forceReload:true);
        }
    }
}
