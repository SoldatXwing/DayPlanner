using Carter;
using DayPlanner.Authorization.Interfaces;
using DayPlanner.Authorization.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCarter();
builder.Services.AddScoped<IAuthService>(provider => new AuthService("serviceAccountKey.json"));
builder.Services.AddHttpClient<IJwtProvider, JwtProvider>((sp,httpClient) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    httpClient.BaseAddress = new Uri(configuration["Authentication:TokenUri"]!);
});

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
    {
        jwtOptions.Audience = builder.Configuration["Authentication:Audience"];
        jwtOptions.TokenValidationParameters.ValidIssuer = builder.Configuration["Authentication:ValidIssuer"];
    }); 
builder.Services.AddAuthorization();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication(); 
app.UseAuthorization();
app.MapCarter();

app.UseHttpsRedirection();


app.Run();
