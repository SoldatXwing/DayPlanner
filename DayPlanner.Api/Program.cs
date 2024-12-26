using Carter;
using DayPlanner.Api.Extensions;
using DayPlanner.Authorization.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using DayPlanner.FireStore;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

FireStoreStore d = new(builder.Configuration["FireBase:project_id"]!);
await d.CreateAppointmentAsync(new DayPlanner.Abstractions.Models.Backend.Appointment
{
    Title = "Test",
    Summary = "Test",
    Start = DateTime.UtcNow,
    End = DateTime.UtcNow.AddHours(1),
    Location = null,
    UserId = "1"
});

var app = builder.Build();

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
