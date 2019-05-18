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
            try
            {
                var pathToExe = Assembly.GetEntryAssembly().Location;
                var pathToUpdater = Path.GetDirectoryName(pathToExe) + @"\Updater.exe";
                Process process = Process.Start(pathToUpdater);
            }
            catch
            {
                //StandardOutput = "Exception. Processed command: " + command;     

            }
        }
    }
}
