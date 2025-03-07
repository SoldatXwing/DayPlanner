using DayPlanner.Web.Wasm;
using DayPlanner.Web.Wasm.Extensions;
using DayPlanner.Web.Wasm.Services;
using DayPlanner.Web.Wasm.Services.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Localization;
using Radzen;
using System.Globalization;
using Blazored.LocalStorage;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// Füge den HttpClient hinzu, der die Basisadresse der WASM-App verwendet (für statische Dateien)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddRefitClients(builder.Configuration);

builder.Services.AddHttpClient("GeoApifyClient", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["GeoApifyApi:HttpClient:BaseAddress"] ?? throw new NotImplementedException("GeoApify API key isn't set. Config path: GeoApifyApi:HttpClient:BaseAddress"));
});

builder.Services.AddRadzenComponents();

builder.Services.AddScoped<IAppointmentService, ApiAppointmentService>();
builder.Services.AddScoped<IUserService, ApiUserService>();
builder.Services.AddScoped<IGoogleCalendarService, ApiGoogleCalendarService>();
builder.Services.AddScoped<IAiService, ApiAiService>();

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<DefaultAuthenticationService>()
    .AddScoped<IAuthenticationService>(sp => sp.GetRequiredService<DefaultAuthenticationService>())
    .AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<DefaultAuthenticationService>());

builder.Services.AddAuthorizationCore(); 


builder.Services.AddCascadingAuthenticationState();

builder.Services.AddLocalization(options => options.ResourcesPath = "Localization");

var app = builder.Build();

await ConfigureCultureAsync(app);

await app.RunAsync();
static async Task ConfigureCultureAsync(WebAssemblyHost app)
{
    var localStorage = app.Services.GetRequiredService<ILocalStorageService>();
    var jsRuntime = app.Services.GetRequiredService<IJSRuntime>();

    var supportedCultures = new[] { "en", "de" };
    var savedCulture = await localStorage.GetItemAsync<string>("culture");

    string selectedCulture;
    if (!string.IsNullOrEmpty(savedCulture) && supportedCultures.Contains(savedCulture))
    {
        selectedCulture = savedCulture;
    }
    else
    {
        var browserCulture = await jsRuntime.InvokeAsync<string>("eval", "navigator.language");
        var browserCultureShort = browserCulture.Split('-').FirstOrDefault();

        selectedCulture = supportedCultures.Contains(browserCultureShort) ? browserCultureShort : "en";
        await localStorage.SetItemAsync("culture", selectedCulture);
    }

    var culture = new CultureInfo(selectedCulture);
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
}
