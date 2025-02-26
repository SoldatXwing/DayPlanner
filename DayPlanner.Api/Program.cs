using DayPlanner.Api.Extensions;
using DayPlanner.Api.Middleware;
using DayPlanner.Api.Swagger;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();
try
{
    logger.Info("Starting application");
    var builder = WebApplication.CreateBuilder(args);

    builder.AddInfrastructure();
    builder.ConfigureAiSupport();


    //Logging
    builder.Logging.ClearProviders();
    builder.Logging.AddNLog();

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
    builder.AddServiceDefaults();

    var app = builder.Build();

    app.MapDefaultEndpoints();
    // Use TraceId middleware
    app.UseMiddleware<TraceIdMiddleware>();

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

}
catch (Exception ex)
{
    logger.Error(ex, "Application stopped due to an exception");
    throw;
}
finally
{
    LogManager.Shutdown(); // Ensure that NLog releases resources
}

