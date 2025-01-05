using Carter;
using DayPlanner.Api.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure();
builder.Services.AddControllers();
builder.Services.AddApiVersioning(options => options.ReportApiVersions = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swagger =>
{
    //This is to generate the Default UI of Swagger Documentation
    swagger.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Day Planner Web API",
        Description = ""
    });
    // To Enable authorization using Swagger (JWT)
    swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = $"Enter ‘Bearer’ [space] and then your valid token in the text input below.{Environment.NewLine}Example: " +
                        $"{Environment.NewLine}Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9"
    });
    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, Array.Empty<string>()
        }
    });
});
//FireStoreStore d = new(builder.Configuration["FireBase:project_id"]!);
//await d.CreateAppointmentAsync(new DayPlanner.Abstractions.Models.Backend.Appointment
//{
//    Title = "Test",
//    Summary = "Test",
//    Start = DateTime.UtcNow,
//    End = DateTime.UtcNow.AddHours(1),
//    Location = null,
//    UserId = "1"
//});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();
app.MapControllers();
app.UseHttpsRedirection();


app.Run();
