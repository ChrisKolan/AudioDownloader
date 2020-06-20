using Logger;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

namespace Updater
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types")]
    class ViewModel : INotifyPropertyChanged
    {
        private bool _exceptionOccured;
        private string _standardOutput;
        private int _progressBarPercent;
        private bool _isIndeterminate;
        private double _taskBarProgressValue;
        private TaskbarItemProgressState _taskbarItemProgressStateModel;
        private static readonly string _pathToExe = Assembly.GetEntryAssembly().Location;
        private static readonly string _pathToExeFolder = Path.GetDirectoryName(_pathToExe);

        public ViewModel()
        {
            IsIndeterminate = true;
            TaskbarItemProgressStateModel = TaskbarItemProgressState.Indeterminate;
            CountdownTimer = new Timer(_ => CountdownUntillExitApplication(), null, TimeSpan.FromSeconds(5) , TimeSpan.FromSeconds(5));
            Task.Run(() => Update());
        }

        public static ILogger Log { get; set; }
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
        public double TaskBarProgressValue
        {
            get { return _taskBarProgressValue; }
            set
            {
                _taskBarProgressValue = value;
                OnPropertyChanged(nameof(TaskBarProgressValue));
            }
        }
        public TaskbarItemProgressState TaskbarItemProgressStateModel
        {
            get { return _taskbarItemProgressStateModel; }
            set
            {
                _taskbarItemProgressStateModel = value;
                OnPropertyChanged(nameof(TaskbarItemProgressStateModel));
            }
        }
        private void Update()
        {
            try
            {
                CreateLogger();
                StopAudioDownloader();
                DeleteOldFiles();
                StartAudioDownloader();
            }
            catch (Exception excetion)
            {
                Log.Error(excetion, Localization.Properties.Resources.UpdaterFailedToUpdate);
                StandardOutput = Localization.Properties.Resources.UpdaterFailedToUpdate;
                TaskbarItemProgressStateModel = TaskbarItemProgressState.Error;
                _exceptionOccured = true;
            }
        }
        private static double GetTaskBarProgressValue(int maximum, int progress)
        {
            return (double)progress / (double)maximum;
        }
        private static void CreateLogger()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var upadterPath = @"\AudioDownloader\Updater.txt";
            Log = LoggerSerilog.Create(appDataPath + upadterPath);
        }
        private void StopAudioDownloader()
        {
            StandardOutput = Localization.Properties.Resources.UpdaterUpdatingAudioDownloader;
            Log.Information(StandardOutput);
            Thread.Sleep(1000);
            IsIndeterminate = false;
            TaskbarItemProgressStateModel = TaskbarItemProgressState.Normal;
            StandardOutput = Localization.Properties.Resources.UpdaterClosingAudioDownloader;
            Log.Information(StandardOutput);
            foreach (var process in Process.GetProcessesByName("AudioDownloader"))
            {
                process.Kill();
                Log.Information("Killed process: {0}", process.ToString());
            }
            ProgressBarPercent = 33;
            TaskBarProgressValue = GetTaskBarProgressValue(99, ProgressBarPercent);
            Thread.Sleep(1000);
        }
        private void DeleteOldFiles()
        {
            StandardOutput = Localization.Properties.Resources.UpdaterDeletingOldFiles;
            Log.Information(StandardOutput);
            DirectoryInfo directoryInfo = new DirectoryInfo(_pathToExeFolder);
            FileInfo[] fileInfoArray = directoryInfo.GetFiles("old_*.*");
            foreach (FileInfo file in fileInfoArray)
            {
                try
                {
                    file.Delete();
                    Log.Information("Deleted file: {0}", file.FullName);
                }
                catch (UnauthorizedAccessException unauthorizedAccessException)
                {
                    Log.Error(unauthorizedAccessException, "Exception during deleting files");
                    continue;
                }
            }
            foreach (DirectoryInfo subDirectories in directoryInfo.GetDirectories())
            {
                FileInfo[] infos = subDirectories.GetFiles("old_*.*");
                foreach (FileInfo file in infos)
                {
                    try
                    {
                        file.Delete();
                        Log.Information("Deleted file: {0}", file.FullName);
                    }
                    catch (UnauthorizedAccessException unauthorizedAccessException)
                    {
                        Log.Error(unauthorizedAccessException, "Exception during deleting files");
                        continue;
                    }
                }
            }
            ProgressBarPercent = 66;
            TaskBarProgressValue = GetTaskBarProgressValue(99, ProgressBarPercent);
            Thread.Sleep(1000);
        }
        private void StartAudioDownloader()
        {
            StandardOutput = Localization.Properties.Resources.UpdaterStartingAudioDownloader;
            Log.Information(StandardOutput);
            var pathToAudioDownloader = _pathToExeFolder + @"\AudioDownloader.exe";
            Process.Start(pathToAudioDownloader);
            ProgressBarPercent = 99;
            TaskBarProgressValue = GetTaskBarProgressValue(99, ProgressBarPercent);
            Thread.Sleep(1000);
            StandardOutput = Localization.Properties.Resources.UpdaterFinishingUpdate;
            Log.Information(StandardOutput);
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
