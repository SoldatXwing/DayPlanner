using Blazored.LocalStorage;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Web.Wasm.Extensions;
using DayPlanner.Web.Wasm.Services.Implementations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Radzen;
using Radzen.Blazor;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

namespace DayPlanner.Web.Wasm.Components.Layouts
{
    public partial class MainLayout
    {
        #region Injections
        [Inject]
        private AuthenticationStateProvider StateProvider { get; set; } = default!;
        [Inject]
        private IStringLocalizer<MainLayout> Localizer { get; set; } = default!;
        [Inject]
        private NavigationManager Navigation { get; set; } = default!;
        [Inject]
        private ILocalStorageService LocalStorageService { get; set; } = default!;
        [Inject]
        private DefaultAuthenticationService AuthenticationService { get; set; } = default!;
        #endregion
        private bool sidebar1Expanded = true;
        private bool IsLightTheme = true;
        User? _user;

        async Task OnChange(bool value)
        {
            await LocalStorageService.SetItemAsync("IsLight", value);
            IsLightTheme = value;
        }
        protected async override Task OnInitializedAsync()
        {
            IsLightTheme = await LocalStorageService.GetItemAsync<bool>("IsLightTheme");
            await UpdateUserAsync();

            StateProvider.AuthenticationStateChanged += async (task) => await UpdateUserAsync();
        }

        private async Task UpdateUserAsync()
        {
            var authState = await StateProvider.GetAuthenticationStateAsync();
            _user = authState.User.Identity?.IsAuthenticated == true ? authState.User.ToUser() : null;

            StateHasChanged(); 
        }


        private string GetUserAvatar()
        {
            if (_user is null || string.IsNullOrWhiteSpace(_user.DisplayName))
                return "U"; // Standard-Avatar, wenn kein Benutzername vorhanden ist.
            if (!string.IsNullOrEmpty(_user.PhotoUrl))
            {
                return _user.PhotoUrl;
            }
            var parts = _user.DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = string.Concat(parts.Select(part => part[0]).Take(2)); 
            return initials.ToUpper(); 
        }
        private async Task Logout()
        {
            if (_user is null)
                return;
            await AuthenticationService.SignOutAsync();
            Navigation.NavigateToHome();
        }
    }
}
