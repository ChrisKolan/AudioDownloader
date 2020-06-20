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
                    rollingInterval: RollingInterval.Month,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

            return Log.Logger;
        }
    }
}
