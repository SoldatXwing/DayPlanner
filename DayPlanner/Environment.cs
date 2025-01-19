using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner;

internal static class Environment
{
    public static string CurrentEnv =>
#if DEBUG
    "Development"
#else
        "Production"
#endif
    ;

    public static DevicePlatform Platform => DeviceInfo.Current.Platform;
}
