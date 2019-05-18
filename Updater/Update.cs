using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Updater
{
    class Update
    {
        private static string _pathToExe = Assembly.GetEntryAssembly().Location;
        private static string _pathToExeFolder = Path.GetDirectoryName(_pathToExe);

        static void Main(string[] args)
        {
            try
            {
                StopAudioDownloader();
                DeleteOldFiles();
                StartAudioDownloader();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Updater exception: " + exception);
            }
        }

        private static void StopAudioDownloader()
        {
            Console.WriteLine("Updater started...");
            Console.WriteLine("Closing Audio Downloader.");
            foreach (var process in Process.GetProcessesByName("AudioDownloader"))
            {
                process.Kill();
            }
            Thread.Sleep(1000);
        }

        private static void DeleteOldFiles()
        {
            Console.WriteLine("Deleting old files...");
            DirectoryInfo directoryInfo = new DirectoryInfo(_pathToExeFolder);
            FileInfo[] fileInfoArray = directoryInfo.GetFiles("old_*.*");
            foreach (FileInfo file in fileInfoArray)
            {
                file.Delete();
            }
            Thread.Sleep(1000);
        }
        private static void StartAudioDownloader()
        {
            Console.WriteLine("Starting Audio Downloader.");
            var pathToAudioDownloader = _pathToExeFolder + @"\AudioDownloader.exe";
            Process.Start(pathToAudioDownloader);
        }
    }
}
