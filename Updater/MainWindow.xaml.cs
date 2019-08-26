using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static string _pathToExe = Assembly.GetEntryAssembly().Location;
        private static string _pathToExeFolder = System.IO.Path.GetDirectoryName(_pathToExe);
        //private TextBlock _TextBlock = new TextBlock();

        public MainWindow()
        {
            InitializeComponent();

            
            //_TextBlock.TextWrapping = TextWrapping.Wrap;
            //_TextBlock.Margin = new Thickness(10);
            //_TextBlock.Inlines.Add("Updating AudioDownloader...");
            //Content = _TextBlock;

            //try
            //{
            //    StopAudioDownloader();
            //    DeleteOldFiles();
            //    StartAudioDownloader();
            //}
            //catch (Exception exception)
            //{
            //    Console.WriteLine("\n\nApplication in an inconsistent state. Please download the latest version manually from:\nhttps://chriskolan.github.io/audio-downloader/\n\n");
            //    StopAudioDownloader();
            //    Console.WriteLine("\n\nPlease consider opening an issue and pasting the exception description at:\nhttps://github.com/ChrisKolan/audio-downloader/issues");
            //    Console.WriteLine("\nUpdater exception: " + exception);
            //    Console.WriteLine("\n\nPress any key to close this window.");
            //    Console.ReadKey();
            //}
            //_TextBlock.Inlines.Add("\nUpdated!");
        }

        private void StopAudioDownloader()
        {
            //_TextBlock.Inlines.Add("\nClosing Audio Downloader.");
            foreach (var process in Process.GetProcessesByName("AudioDownloader"))
            {
                process.Kill();
            }
            Thread.Sleep(100);
        }

        private void DeleteOldFiles()
        {
            //_TextBlock.Inlines.Add("\nDeleting old files.");
            DirectoryInfo directoryInfo = new DirectoryInfo(_pathToExeFolder);
            FileInfo[] fileInfoArray = directoryInfo.GetFiles("old_*.*");
            foreach (FileInfo file in fileInfoArray)
            {
                try
                {
                    file.Delete();
                }
                catch (System.UnauthorizedAccessException)
                {
                    continue;
                }
            }
            Thread.Sleep(100);
        }
        private void StartAudioDownloader()
        {
            //_TextBlock.Inlines.Add("\nStarting Audio Downloader.");
            var pathToAudioDownloader = _pathToExeFolder + @"\AudioDownloader.exe";
            Process.Start(pathToAudioDownloader);
        }
    }
}
