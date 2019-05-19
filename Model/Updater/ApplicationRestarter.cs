using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace Model
{
    class ApplicationRestarter
    {
        public static void Restart()
        {
            var pathToExe = Assembly.GetEntryAssembly().Location;
            var pathToUpdater = Path.GetDirectoryName(pathToExe) + @"\Updater.exe";
            Process.Start(pathToUpdater);
        }
    }
}
