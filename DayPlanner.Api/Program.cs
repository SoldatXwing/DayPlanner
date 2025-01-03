using DayPlanner.Api.Extensions;
using DayPlanner.Api.Swagger;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure();

builder.Services.AddControllers();

builder.Services
    .AddApiVersioning(options => options.ReportApiVersions = true)
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

if (builder.Configuration.GetValue<bool?>("Swagger:Enabled") ?? false)
{
    builder.Services.AddSwaggerGen();
    builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwagger>();

    if (builder.Configuration.GetValue<bool?>("Swagger:UiEnabled") ?? false)
    {
        builder.Services.AddTransient<IConfigureOptions<SwaggerUIOptions>, ConfigureSwaggerUi>();
    }
}

var app = builder.Build();
app.UseHttpsRedirection();

if (app.Configuration.GetValue<bool?>("Swagger:Enabled") ?? false)
{
    app.UseSwagger();

    if (app.Configuration.GetValue<bool?>("Swagger:UiEnabled") ?? false)
        app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization(defaultPolicy =>
{
    defaultPolicy.RequireAuthenticatedUser();
    defaultPolicy.RequireAssertion(context => !string.IsNullOrEmpty(context.User.GetUserId()));
});

app.Run();
