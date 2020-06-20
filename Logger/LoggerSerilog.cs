using Serilog;

namespace Logger
{
    public static class LoggerSerilog
    {
        public static ILogger Create(string path)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(path,
                    rollingInterval: RollingInterval.Month,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

            return Log.Logger;
        }
    }
}
