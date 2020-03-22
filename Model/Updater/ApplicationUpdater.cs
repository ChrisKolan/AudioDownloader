using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types")]
    class ApplicationUpdater
    {
        public static async Task UpdateAsync(Model model)
        {
            if (UpdatesNeeded(out ConfigurationErrorsException configurationErrorsException))
            {
                model.StandardOutput = "Checking for updates";
                model.DisableInteractions();
                model.ExceptionHandler = configurationErrorsException;

                try
                {
                    await UpdatesDownloader.DownloadUpdatesAsync(model);
                }
                catch (Exception exception)
                {
                    model.DownloadLinkEnabled = true;
                    model.DownloadLinkTextDecorations = null;
                    model.StandardOutput = "Failed to update. Click here to download manually.";
                    model.GetLocalVersions();
                    model.EnableInteractions();
                    model.ExceptionHandler = exception;
                    return;
                }
            }

            model.StandardOutput = "Ready";
            model.GetLocalVersions();
            model.EnableInteractions();
        }

        private static bool TryAddOrUpdateApplicationSettings(string key, string value, out ConfigurationErrorsException configurationErrorsException)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);

                configurationErrorsException = new ConfigurationErrorsException("Configuration updated");
                return true;
            }
            catch (ConfigurationErrorsException exception)
            {
                configurationErrorsException = exception;
                return false;
            }
        }
        private static bool UpdatesNeeded(out ConfigurationErrorsException configurationErrorsException)
        {
            double updateIfElapsedTimeExceeded = 10; 
            DateTime lastStoredUpdateDateTimeFromBinary;
            string lastStoredUpdateDateTime = ConfigurationManager.AppSettings["UpdateDateTime"];
            if (lastStoredUpdateDateTime != null)
            {
                var lastStoredUpdateDateTimeLong = long.Parse(lastStoredUpdateDateTime);
                lastStoredUpdateDateTimeFromBinary = DateTime.FromBinary(lastStoredUpdateDateTimeLong);
                var currentUpdateDateTime = UpdateSettings(out ConfigurationErrorsException configurationErrorsExceptionInternal);
                var elapsedTimeBetweenUpdates = currentUpdateDateTime.Subtract(lastStoredUpdateDateTimeFromBinary).TotalMinutes;
                configurationErrorsException = configurationErrorsExceptionInternal;
                return elapsedTimeBetweenUpdates > updateIfElapsedTimeExceeded;
            }
            else
            {
                UpdateSettings(out ConfigurationErrorsException configurationErrorsExceptionInternal);
                configurationErrorsException = configurationErrorsExceptionInternal;
                return true;
            }
        }
        private static DateTime UpdateSettings(out ConfigurationErrorsException configurationErrorsException)
        {
            var currentUpdateDateTime = DateTime.Now;
            var currentUpdateDateTimeString = currentUpdateDateTime.ToBinary().ToString();
            TryAddOrUpdateApplicationSettings("UpdateDateTime", currentUpdateDateTimeString, out ConfigurationErrorsException configurationErrorsExceptionInternal);
            configurationErrorsException = configurationErrorsExceptionInternal;

            return currentUpdateDateTime;
        }
    }
}
