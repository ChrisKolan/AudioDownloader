using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Updater
{
    class ViewModel : INotifyPropertyChanged
    {
        private bool _exceptionOccured;
        private string _standardOutput;
        private int _progressBarPercent;
        private bool _isIndeterminate;
        private static readonly string _pathToExe = Assembly.GetEntryAssembly().Location;
        private static readonly string _pathToExeFolder = Path.GetDirectoryName(_pathToExe);

        public ViewModel()
        {
            IsIndeterminate = true;
            CountdownTimer = new Timer(_ => CountdownUntillExitApplication(), null, TimeSpan.FromSeconds(5) , TimeSpan.FromSeconds(5));
            Task.Run(() => Update());
        }

        public Timer CountdownTimer { get; }
        public Timer StandardOutputUpdateTimer { get; }
        public string StandardOutput
        {
            get { return _standardOutput; }
            set
            {
                _standardOutput = value;
                OnPropertyChanged(nameof(StandardOutput));
            }
        }
        public int ProgressBarPercent
        {
            get { return _progressBarPercent; }
            set
            {
                _progressBarPercent = value;
                OnPropertyChanged(nameof(ProgressBarPercent));
            }
        }
        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set
            {
                _isIndeterminate = value;
                OnPropertyChanged(nameof(IsIndeterminate));
            }
        }

        private void Update()
        {
            try
            {
                StopAudioDownloader();
                DeleteOldFiles();
                StartAudioDownloader();
            }
            catch (Exception)
            {
                StandardOutput = "Failed to update. Please download the latest version manually from: https://chriskolan.github.io/audio-downloader";
                _exceptionOccured = true;
            }
        }
        private void StopAudioDownloader()
        {
            StandardOutput = "Updating Audio Downloader...";
            Thread.Sleep(1000);
            StandardOutput = "Closing Audio Downloader.";
            foreach (var process in Process.GetProcessesByName("AudioDownloader"))
            {
                process.Kill();
            }
            Thread.Sleep(1000);
        }
        private void DeleteOldFiles()
        {
            StandardOutput = "Deleting old files.";
            DirectoryInfo directoryInfo = new DirectoryInfo(_pathToExeFolder);
            FileInfo[] fileInfoArray = directoryInfo.GetFiles("old_*.*");
            foreach (FileInfo file in fileInfoArray)
            {
                try
                {
                    file.Delete();
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
            }
            Thread.Sleep(1000);
        }
        private void StartAudioDownloader()
        {
            StandardOutput = "Starting Audio Downloader.";
            var pathToAudioDownloader = _pathToExeFolder + @"\AudioDownloader.exe";
            Process.Start(pathToAudioDownloader);
            Thread.Sleep(1000);
            StandardOutput = "Closing Audio Downloader Updater.";
        }
        private void CountdownUntillExitApplication()
        {
            if (_exceptionOccured)
            {
                return;
            }
            Application.Current.MainWindow.Close();
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
