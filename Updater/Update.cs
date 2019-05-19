using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

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
                Console.WriteLine("\n\nApplication in an inconsistent state. Please download the latest version manually from:\nhttps://chriskolan.github.io/audio-downloader/\n\n");
                StopAudioDownloader();
                Console.WriteLine("\n\nPlease consider opening an issue and pasting the exception description at:\nhttps://github.com/ChrisKolan/audio-downloader/issues");
                Console.WriteLine("\nUpdater exception: " + exception);
                Console.WriteLine("\n\nPress any key to close this window.");
                Console.ReadKey();
            }
        }

        private static void StopAudioDownloader()
        {
            Console.WriteLine("Closing Audio Downloader.");
            foreach (var process in Process.GetProcessesByName("AudioDownloader"))
            {
                process.Kill();
            }
            Thread.Sleep(100);
        }

        private static void DeleteOldFiles()
        {
            Console.WriteLine("Deleting old files.");
            DirectoryInfo directoryInfo = new DirectoryInfo(_pathToExeFolder);
            FileInfo[] fileInfoArray = directoryInfo.GetFiles("old_*.*");
            foreach (FileInfo file in fileInfoArray)
            {
                file.Delete();
            }
            Thread.Sleep(100);
        }
        private static void StartAudioDownloader()
        {
            Console.WriteLine("Starting Audio Downloader.");
            var pathToAudioDownloader = _pathToExeFolder + @"\AudioDownloader.exe";
            Process.Start(pathToAudioDownloader);
        }
    }
}
