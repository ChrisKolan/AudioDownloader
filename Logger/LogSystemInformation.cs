using Serilog;
using System;
using System.Globalization;

namespace Logger
{
    public static class LogSystemInformation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static void Log(ILogger log)
        {
            CultureInfo cultureInfo = CultureInfo.InstalledUICulture;
            log.Information("==========================================================================");
            log.Information("CultureInfo name: {0}", cultureInfo.Name);
            log.Information("CultureInfo display name: {0}", cultureInfo.DisplayName);
            try
            {
                log.Information("Framework version: {0}", GetDotNetVersion.Version());
                log.Information("Operating system version: {0}", GetWindowsVersion.OsVersion);
                log.Information("Windows release ID: {0}", GetWindowsVersion.ReleaseId);
            }
            catch (Exception exception)
            {
                log.Error(exception, "Exception in getting system information");
            }
        }
    }
}
