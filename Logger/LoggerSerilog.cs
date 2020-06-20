using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Logger
{
    public static class LoggerSerilog
    {
        public static ILogger Create()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("log.txt",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

            CultureInfo cultureInfo = CultureInfo.InstalledUICulture;

            Log.Information("==========================================================================");
            Log.Information("CultureInfo name: {0}", cultureInfo.Name);
            Log.Information("CultureInfo display name: {0}", cultureInfo.DisplayName);
            Log.Information("Framework version: {0}", GetDotNetVersion.Version());

            return Log.Logger;
        }
    }
}
