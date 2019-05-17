using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Voltsoft.FileVersionUtilities;

namespace ViewModel
{
    public class LocalVersionProvider
    {
        public static List<string>Versions()
        {
            var localVersions = new List<string>();
            var localVersionsToCheck = new List<string>();

            var pathToExe = Assembly.GetEntryAssembly().Location;
            var pathToExeFolder = System.IO.Path.GetDirectoryName(pathToExe);
            var pathToYoutubeDl = pathToExeFolder + @"\bin\youtube-dl.exe";
            localVersionsToCheck.Add(pathToYoutubeDl);
            localVersionsToCheck.Add(pathToExe);

            foreach (var localVersion in localVersionsToCheck)
            {
                localVersions.Add(Utilities.GetFileVersion(localVersion));
            }

            return localVersions;
        }
    }
}
