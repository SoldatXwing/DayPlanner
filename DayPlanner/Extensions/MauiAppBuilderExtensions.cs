using Microsoft.Extensions.Configuration;

namespace DayPlanner.Extensions;

internal static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Adds json configuration to the app builder.
    /// </summary>
    /// <param name="builder">The builder to use.</param>
    /// <param name="settingsFile">The name of the default settings file.</param>
    /// <returns>The builder pipeline.</returns>
    public static MauiAppBuilder AddJsonConfiguration(this MauiAppBuilder builder, string settingsFile, bool optional = true)
    {
        ArgumentNullException.ThrowIfNull(builder);

        Stream? settingsStream = typeof(MauiProgram).Assembly.GetManifestResourceStream(settingsFile);
        if (!optional && settingsStream is null)
        {
            throw new FileNotFoundException($"Unable to find json configuration resource '{settingsFile}'.");
        }

        if (settingsStream is not null)
            builder.Configuration.AddJsonStream(settingsStream);

        return builder;
    }
}
