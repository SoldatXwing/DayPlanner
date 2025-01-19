using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace DayPlanner.Components.Layouts;

public sealed partial class MainLayout : LayoutComponentBase, IDisposable
{
    #region Injections
    [Inject]
    private IStringLocalizer<MainLayout> Localizer { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider StateProvider { get; set; } = default!;
    #endregion

    private User? _user;
    private bool _isDarkMode;

    public const string _darkModePrefKey = "isDarkMode";

    protected override void OnInitialized()
    {
        _isDarkMode = Preferences.Default.Get(_darkModePrefKey, false);

        StateProvider.AuthenticationStateChanged += OnUpdateAuthenticationState;
        OnUpdateAuthenticationState(StateProvider.GetAuthenticationStateAsync());
    }

    private void ThemeMode_OnToggledChanged(bool newValue)
    {
        _isDarkMode = newValue;
        Preferences.Default.Set(_darkModePrefKey, newValue);
    }

    private string GetUserAvatar()
    {
        if (_user is null)
            throw new InvalidOperationException();

        char[] chars = !string.IsNullOrWhiteSpace(_user.DisplayName)
            ? [.. _user.DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(part => part[0]).Take(2)]
            : ['U'];
        return string.Concat(chars);
    }

    private async void OnUpdateAuthenticationState(Task<AuthenticationState> stateTask)
    {
        AuthenticationState state = await stateTask;
        _user = state.User.Identity?.IsAuthenticated ?? false
            ? _user = state.User.ToUser()
            : null;
    }

    public void Dispose() => StateProvider.AuthenticationStateChanged -= OnUpdateAuthenticationState;
}
