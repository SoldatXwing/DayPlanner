using DayPlanner.Web.Components;
using DayPlanner.Web.Extensions;
using DayPlanner.Web.Services.Implementations;
using Microsoft.AspNetCore.Identity;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRefitClients(builder.Configuration);
builder.Services.AddRadzenComponents();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<DefaultAuthenticationService>()
    .AddScoped<DayPlanner.Web.Services.IAuthenticationService>(sp => sp.GetRequiredService<DefaultAuthenticationService>());

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignOutScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie(IdentityConstants.ApplicationScheme, options =>
{
    options.LoginPath = "/account/login"; // Login-Seite
    options.LogoutPath = "/account/logout"; // Logout-Seite
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Cookie-Ablaufzeit
    options.SlidingExpiration = true; // Verlängert Cookie bei Aktivität
    options.AccessDeniedPath = "/account/access-denied"; // Zugriff verweigert

    options.ReturnUrlParameter = "returnUrl";
});
builder.Services.AddAuthorizationCore();

builder.Services.AddLocalization(options => options.ResourcesPath = "Localization");
builder.Services.AddRequestLocalization(options =>
{
    string[] cultures = ["en", "de"];
    options.SetDefaultCulture(cultures[0])
        .AddSupportedCultures(cultures)
        .AddSupportedUICultures(cultures);
    options.ApplyCurrentCultureToResponseHeaders = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization();

app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
