using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Model
{
    class ApplicationUpdater
    {
        public static async Task UpdateAsync(Model model)
        {
            if (UpdatesNeeded())
            {
                model.StandardOutput = "Checking for updates";
                model.DisableInteractions();

                try
                {
                    await UpdatesDownloader.DownloadUpdatesAsync(model);
                }
                catch (Exception)
                {
                    model.DownloadLinkEnabled = true;
                    model.DownloadLinkTextDecorations = TextDecorations.Underline;
                    model.StandardOutput = "Failed to update. Click here to download manually.";
                    model.GetLocalVersions();
                    model.EnableInteractions();
                    return;
                }
            }

            model.StandardOutput = "Ready";
            model.GetLocalVersions();
            model.EnableInteractions();
        }

        private static bool TryAddOrUpdateApplicationSettings(string key, string value)
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

                return true;
            }
            catch (ConfigurationErrorsException)
            {
                return false;
            }
        }
        private static bool UpdatesNeeded()
        {
            double updateIfElapsedTimeExceeded = 10; 
            DateTime lastStoredUpdateDateTimeFromBinary;
            string lastStoredUpdateDateTime = ConfigurationManager.AppSettings["UpdateDateTime"];
            if (lastStoredUpdateDateTime != null)
            {
                var lastStoredUpdateDateTimeLong = long.Parse(lastStoredUpdateDateTime);
                lastStoredUpdateDateTimeFromBinary = DateTime.FromBinary(lastStoredUpdateDateTimeLong);
                var currentUpdateDateTime = UpdateSettings();
                var elapsedTimeBetweenUpdates = currentUpdateDateTime.Subtract(lastStoredUpdateDateTimeFromBinary).TotalMinutes;

                return elapsedTimeBetweenUpdates > updateIfElapsedTimeExceeded;
            }
            else
            {
                UpdateSettings();

                return true;
            }
        }
        private static DateTime UpdateSettings()
        {
            var currentUpdateDateTime = DateTime.Now;
            var currentUpdateDateTimeString = currentUpdateDateTime.ToBinary().ToString();
            TryAddOrUpdateApplicationSettings("UpdateDateTime", currentUpdateDateTimeString);

            return currentUpdateDateTime;
        }
    }
}
