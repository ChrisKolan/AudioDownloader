using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    class ApplicationRestarter
    {
        public static void Restart()
        {
            try
            {
                Process process = Process.Start("CMD.exe");
                process.WaitForExit();
            }
            catch
            {
                //StandardOutput = "Exception. Processed command: " + command;     

            }
        }
    }
}
