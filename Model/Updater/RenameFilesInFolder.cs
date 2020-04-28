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
        public static void Rename(ModelClass model)
        {
            model.InformationAndExceptionOutput = "Renaming files";
            var prefix = "old_";
            var pathToExe = Assembly.GetEntryAssembly().Location;
            var pathToExeFolder = Path.GetDirectoryName(pathToExe) + @"\";
            DirectoryInfo directoryInfo = new DirectoryInfo(pathToExeFolder);
            FileInfo[] fileInfoArray = directoryInfo.GetFiles();
            foreach (FileInfo file in fileInfoArray)
            {
                var fileString = file.ToString();
                model.InformationAndExceptionOutput = "Renaming file: " + fileString;
                if (fileString.Contains("AudioDownloader.zip"))
                {
                    continue;
                }
                var newFileName = Path.Combine(Path.GetDirectoryName(file.ToString()), (prefix + Path.GetFileName(file.ToString())));
                File.Move(pathToExeFolder + file.ToString(), pathToExeFolder + newFileName);
            }
        }
    }
}
