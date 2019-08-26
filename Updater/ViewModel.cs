using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Updater
{
    class ViewModel : INotifyPropertyChanged
    {
        private string _standardOutput;
        private static string _pathToExe = Assembly.GetEntryAssembly().Location;
        private static string _pathToExeFolder = System.IO.Path.GetDirectoryName(_pathToExe);

        public ViewModel()
        {
            StandardOutput = "Test";
        }

        public string StandardOutput
        {
            get { return _standardOutput; }
            set
            {
                _standardOutput = value;
                OnPropertyChanged(nameof(StandardOutput));
            }
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

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
