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
    public static MauiAppBuilder AddJsonConfiguration(this MauiAppBuilder builder, string settingsFile)
    {
        ArgumentNullException.ThrowIfNull(builder);

        Stream settingsStream = typeof(MauiProgram).Assembly.GetManifestResourceStream(settingsFile)
            ?? throw new FileNotFoundException($"Unable to find settings file {settingsFile} in manifest.");
        builder.Configuration.AddJsonStream(settingsStream);

        return builder;
    }
}
