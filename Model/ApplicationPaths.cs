using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public static class ApplicationPaths
    {
        private static string _AudioPath;
        public static string AudioVideoPath { get; set; }

        public static string GetAudioPath()
        {
            _AudioPath = ApplicationSettingsProvider.GetValue("ApplicationStoragePath");
            if (_AudioPath == null)
            {
                var pathToAudioFolder = PathToExeFolder() + @"\audio\";
                _AudioPath = pathToAudioFolder;
            }
            AudioVideoPath = _AudioPath + @"\video\";
            return _AudioPath;
        }

        public static void SetAudioPath(string userChosenPath)
        {
            ApplicationSettingsProvider.TryAddOrUpdateApplicationSettings("ApplicationStoragePath", userChosenPath + "\\", out string configurationErrorsException);
        }

        public static string PathToExeFolder()
        {
            var pathToExe = Assembly.GetEntryAssembly().Location;
            return Path.GetDirectoryName(pathToExe);
        }
    }
}
