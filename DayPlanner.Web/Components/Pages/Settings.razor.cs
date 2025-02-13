using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

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
        #endregion

        private User? User;
        private UpdateUserRequest? UserRequest;
        #region Google properties
        private bool isGoogleConnected = false;
        private string googleConnectionStatus = string.Empty;
        #endregion

        private bool editMode = false;

        private void CancelEdit()
        {
            editMode = false;
            UserRequest!.Email = User!.Email!;
            UserRequest!.DisplayName = User!.DisplayName!;
        }
        protected override async Task OnInitializedAsync()
        {
            var authState = await StateProvider.GetAuthenticationStateAsync();
            User = authState.User.Identity?.IsAuthenticated ?? false
                ? authState.User.ToUser()
                : null;

            UserRequest = new()
            {
                Email = User!.Email!,
                DisplayName = User!.DisplayName!
            };
        }

        private async Task SaveUserSettings()
        {

        }

        private async Task ConnectGoogleCalendar()
        {

        }

        private async Task DisconnectGoogle()
        {

        }
    }
}
