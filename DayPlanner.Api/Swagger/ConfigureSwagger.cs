using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DayPlanner.Api.Swagger;

internal class ConfigureSwagger(IApiVersionDescriptionProvider provider, IConfiguration configuration) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        IConfigurationSection openApiConfig = configuration.GetRequiredSection("OpenApi");
        foreach (ApiVersionDescription apiVersion in provider.ApiVersionDescriptions)
        {
            OpenApiInfo versionInfo = new()
            {
                Title = openApiConfig["Title"],
                Description = openApiConfig["Description"],
                Version = apiVersion.ApiVersion.ToString(),
                TermsOfService = openApiConfig.GetValue<Uri>("TermsOfService")
            };
            if (openApiConfig.GetSection("License").Exists())
            {
                versionInfo.License = new()
                {
                    Name = openApiConfig["License:Name"],
                    Url = openApiConfig.GetValue<Uri>("License:Url")
                };
            }
            if (openApiConfig.GetSection("Contact").Exists())
            {
                versionInfo.Contact = new()
                {
                    Name = openApiConfig["Contact:Name"],
                    Email = openApiConfig["Contact:Email"],
                    Url = openApiConfig.GetValue<Uri>("Contact:Url")
                };
            }

            options.SwaggerDoc(apiVersion.GroupName, versionInfo);
        }

        string fileName = $"{typeof(Program).Assembly.GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, fileName), includeControllerXmlComments: true);

        // To Enable authorization using Swagger (JWT)
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = $"Enter ‘Bearer’ [space] and then your valid token in the text input below.{Environment.NewLine}Example: " +
                            $"{Environment.NewLine}Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }, []
            }
        });
    }
}
