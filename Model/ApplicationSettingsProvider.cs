using System.Configuration;

namespace Model
{
    public static class ApplicationSettingsProvider
    {
        public static bool TryAddOrUpdateApplicationSettings(string key, string value, out string configurationErrorsException)
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

                configurationErrorsException = Localization.Properties.Resources.StatusLogConfigurationUpdated;
                return true;
            }
            catch (ConfigurationErrorsException exception)
            {
                configurationErrorsException = exception.Message;
                return false;
            }
        }
        public static string GetValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
