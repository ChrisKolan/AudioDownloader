using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    class RenameFilesInFolder
    {
        public static void Rename()
        {
            var prefix = "old_";
            var audioFolder = @"\audio\";
            var pathToExe = Assembly.GetEntryAssembly().Location;
            var pathToExeFolder = Path.GetDirectoryName(pathToExe);
            DirectoryInfo directoryInfo = new DirectoryInfo(pathToExeFolder + audioFolder);
            FileInfo[] fileInfoArray = directoryInfo.GetFiles();
            foreach (FileInfo file in fileInfoArray)
            {
                var newFileName = Path.Combine(Path.GetDirectoryName(file.ToString()), (prefix + Path.GetFileName(file.ToString())));
                File.Move(pathToExeFolder + audioFolder + file.ToString(), pathToExeFolder + audioFolder + newFileName);
            }
        }
    }
}
