using System;
using System.Collections.Generic;
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

            return Log.Logger;
        }
    }
}
