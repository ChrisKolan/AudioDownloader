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
        public static string GetAudioPath()
        {
            var path = ApplicationSettingsProvider.GetValue("ApplicationStoragePath");
            if (path == null)
            {
                var pathToAudioFolder = PathToExeFolder() + @"\audio";
                path = pathToAudioFolder;
            }
            return path;
        }

        public static void SetAudioPath(string userChosenPath)
        {
            ApplicationSettingsProvider.TryAddOrUpdateApplicationSettings("ApplicationStoragePath", userChosenPath, out string configurationErrorsException);
        }

        public static string PathToExeFolder()
        {
            var pathToExe = Assembly.GetEntryAssembly().Location;
            return Path.GetDirectoryName(pathToExe);
        }
    }
}
