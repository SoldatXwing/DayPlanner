using Java.Lang;
using Serilog.Core;
using Serilog.Events;
using Android.Util;

namespace DayPlanner;

internal class AndroidEventSink : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        string category = logEvent.Properties[Constants.SourceContextPropertyName].ToString();
        string message = logEvent.RenderMessage();
        Throwable? javaEx = logEvent.Exception is not null
            ? Throwable.FromException(logEvent.Exception)
            : null;

        switch (logEvent.Level)
        {
            case LogEventLevel.Verbose:
                Log.Verbose(category, javaEx!, message);
                break;
            case LogEventLevel.Debug:
                Log.Debug(category, javaEx!, message);
                break;
            case LogEventLevel.Information:
                Log.Info(category, javaEx!, message);
                break;
            case LogEventLevel.Warning:
                Log.Warn(category, javaEx!, message);
                break;
            case LogEventLevel.Error:
                Log.Error(category, javaEx!, message);
                break;
            case LogEventLevel.Fatal:
                Log.Wtf(category, javaEx!, message);
                break;
        }
    }
}
