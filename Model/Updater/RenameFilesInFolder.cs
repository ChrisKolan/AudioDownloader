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
            var prefix = "old_";
            var pathToExe = Assembly.GetEntryAssembly().Location;
            var pathToExeFolder = Path.GetDirectoryName(pathToExe) + @"\";
            DirectoryInfo directoryInfo = new DirectoryInfo(pathToExeFolder);
            FileInfo[] fileInfoArray = directoryInfo.GetFiles();

            foreach (FileInfo file in fileInfoArray)
            {
                var fileString = file.ToString();
                if (fileString.Contains("AudioDownloader.zip"))
                {
                    continue;
                }
                var newFileName = Path.Combine(Path.GetDirectoryName(file.ToString()), (prefix + Path.GetFileName(file.ToString())));
                model.Log.Information("Old file name: {0}, renamed file name: {1}", file.FullName, newFileName);
                File.Move(pathToExeFolder + file.ToString(), pathToExeFolder + newFileName);
            }

            foreach (DirectoryInfo subDirectories in directoryInfo.GetDirectories())
            {
                FileInfo[] infos = subDirectories.GetFiles();
                foreach (FileInfo file in infos)
                {
                    var combinedPath = Path.Combine(file.Directory.ToString(), file.Directory.ToString() + @"\" + prefix + file.Name);
                    model.Log.Information("Old file name: {0}, renamed file name: {1}", file.FullName, combinedPath);
                    File.Move(file.FullName, combinedPath);
                }
            }
        }
    }
}
