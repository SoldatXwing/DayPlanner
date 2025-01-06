using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace DayPlanner.Api.Swagger;

internal class ConfigureSwaggerUi(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerUIOptions>
{
    public void Configure(SwaggerUIOptions options)
    {
        foreach (ApiVersionDescription apiVersion in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{apiVersion.GroupName}/swagger.json", apiVersion.GroupName.ToUpperInvariant());
        }
    }
}
