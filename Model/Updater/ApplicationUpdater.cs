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
        public static async Task UpdateAsync(ModelClass model)
        {
            string localVersions;
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
                    model.StandardOutput = "Failed to update. Click here to download manually.";
                    localVersions = GetLocalVersions();
                    model.LocalVersions = localVersions;
                    model.HelpButtonToolTip = localVersions;
                    model.EnableInteractions();
                    model.ExceptionHandler = exception;
                    return;
                }
            }

            model.StandardOutput = "Ready";
            localVersions = GetLocalVersions();
            model.LocalVersions = localVersions;
            model.HelpButtonToolTip = localVersions;
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

        private static string GetLocalVersions()
        {
            var localVersionsNamesAndNumber = new List<string>
            {
                "Press button to get online help.",
                "===========================",
                "Software \t   |\tVersion",
                "----------------------|-----------------------"
            };
            var localVersions = LocalVersionProvider.Versions();
            var localVersionsSoftwareNames = new List<string>
            {
                "Audio Downloader  |\t",
                "Youtube-dl\t   |\t"
            };

            for (int i = 0; i < localVersions.Count; i++)
            {
                localVersionsNamesAndNumber.Add(localVersionsSoftwareNames[i] + localVersions[i]);
            }

            localVersionsNamesAndNumber.Add("FFmpeg\t\t   |\t4.2.1");

            return String.Join(Environment.NewLine, localVersionsNamesAndNumber.ToArray());
        }
    }
}
