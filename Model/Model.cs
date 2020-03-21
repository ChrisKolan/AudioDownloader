using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;
using Webhook;

namespace Model
{
    public class Model : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Fields
        private string _standardOutput;
        private string _timersOutput;
        private string _buttonContent;
        private static bool _measureProcessingTime;
        private static bool _measureDownloadTime;
        private int _counter;
        private string _finishedMessage;
        private string _downloadedFileSize;
        private int _progressBarPercent;
        private double _taskBarProgressValue;
        private TaskbarItemProgressState _taskbarItemProgressStateModel;
        private bool _isIndeterminate;
        private bool _isButtonEnabled;
        private bool _isInputEnabled;
        private string _selectedQuality;
        private string _helpButtonToolTip;
        private string _downloadLink;
        private bool _isComboBoxEnabled;
        private bool _isAutomaticDownloadSelected;
        private bool _downloadLinkEnabled;
        private double _downloadFileSize;
        private int _audioVideoDownloadCounter;
        private TextDecorationCollection _downloadLinkTextDecorations;
        private int _processingTime = 1;
        private int _downloadTime = 1;
        private int _timerResolution = 100;
        private SynchronizationContext _synchronizationContext;
        private Thread _currentThreadPoolWorker;
        private bool _isDownloadRunning;
        private bool _isOnline;
        private SolidColorBrush _glowBrushColor;
        private bool _isClipboardCaptureSelected;
        private int _pingerCounter;
        #endregion

        #region Constructor
        public Model()
        {
            StandardOutput = "Ready";
            ButtonContent = "Download";
            EnableInteractions();
            PeriodicTimerProcessing = new Timer(_ => ProcessingTimeMeasurement(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_timerResolution));
            PeriodicTimerDownload = new Timer(_ => DownloadTimeMeasurement(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_timerResolution));
            PeriodicTimerPinger = new Timer(_ => TimerPinger(), null, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000));
            PeriodicTimerClipper = new Timer(_ => TimerClipper(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_timerResolution));
            QualityDefault = Helpers.QualityDefault();
            Quality = new ObservableCollection<string>();
            Quality.Add("After pasting YouTube link, you can select the audio quality from this list");
            SelectedQuality = Quality[0];
            _ = ApplicationUpdater.UpdateAsync(this);
            GlowBrushColor = new SolidColorBrush(Colors.LightBlue);
            _synchronizationContext = SynchronizationContext.Current;
        }
        #endregion

        #region Properties

        public List<string> QualityDefault { get; set; }

        public ObservableCollection<string> Quality { get; set; }

        public string LocalVersions { get; set; }

        [Required]
        [CustomValidation(typeof(Model), "ValidateDownloadLink")]
        public string DownloadLink
        {
            get { return _downloadLink; }
            set
            {
                _downloadLink = value;
                OnPropertyChanged(nameof(DownloadLink));
                ValidateAsync();
            }
        }

        public string DownloadedFileSize
        {
            get { return _downloadedFileSize; }
            set 
            { 
                if( _downloadedFileSize != value)
                {
                    if (_audioVideoDownloadCounter < 2)
                    {
                        if (double.TryParse(value.Remove(value.Length - 3), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var downloadedSize))
                        {
                            _downloadFileSize += downloadedSize;
                        }
                        _audioVideoDownloadCounter++;
                    }
                    _downloadedFileSize = _downloadFileSize.ToString() + "MiB";
                }
            }
        }

        public bool DownloadLinkEnabled
        {
            get { return _downloadLinkEnabled; }
            set
            {
                _downloadLinkEnabled = value;
                OnPropertyChanged(nameof(DownloadLinkEnabled));
            }
        }

        public TextDecorationCollection DownloadLinkTextDecorations
        {
            get { return _downloadLinkTextDecorations; }
            set
            {
                _downloadLinkTextDecorations = value;
                OnPropertyChanged(nameof(DownloadLinkTextDecorations));
            }
        }

        public bool IsComboBoxEnabled
        {
            get { return _isComboBoxEnabled; }
            set
            {
                _isComboBoxEnabled = value;
                OnPropertyChanged(nameof(IsComboBoxEnabled));
            }
        }

        public bool IsAutomaticDownloadSelected
        {
            get { return _isAutomaticDownloadSelected; }
            set
            {
                _isAutomaticDownloadSelected = value;
                OnPropertyChanged(nameof(IsAutomaticDownloadSelected));
            }
        }

        public string SelectedQuality
        {
            get { return _selectedQuality; }
            set
            {
                _selectedQuality = value;
                if (value != null)
                {
                    if (IsAutomaticDownloadSelected)
                    {
                        DownloadButtonClick();
                    }
                    OnPropertyChanged(nameof(SelectedQuality));
                }
            }
        }

        public string CurrentStandardOutputLine { get; set; }

        public string StandardOutput
        {
            get { return _standardOutput; }
            set
            {
                _standardOutput = value;
                OnPropertyChanged(nameof(StandardOutput));
            }
        }
        public string TimersOutput
        {
            get { return _timersOutput; }
            set
            {
                _timersOutput = value;
                OnPropertyChanged(nameof(TimersOutput));
            }
        }
        public string ButtonContent
        {
            get { return _buttonContent; }
            set
            {
                _buttonContent = value;
                OnPropertyChanged(nameof(ButtonContent));
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

        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set
            {
                _isIndeterminate = value;
                OnPropertyChanged(nameof(IsIndeterminate));
            }
        }

        public bool IsButtonEnabled
        {
            get { return _isButtonEnabled; }
            set
            {
                _isButtonEnabled = value;
                OnPropertyChanged(nameof(IsButtonEnabled));
            }
        }

        public bool IsInputEnabled
        {
            get { return _isInputEnabled; }
            set
            {
                _isInputEnabled = value;
                OnPropertyChanged(nameof(IsInputEnabled));
            }
        }

        public string HelpButtonToolTip
        {
            get { return _helpButtonToolTip; }
            set
            {
                _helpButtonToolTip = value;
                OnPropertyChanged(nameof(HelpButtonToolTip));
            }
        }

        public SolidColorBrush GlowBrushColor
        {
            get { return _glowBrushColor; }
            set 
            { 
                _glowBrushColor = value;
                OnPropertyChanged(nameof(GlowBrushColor));
            }
        }

        public bool IsClipboardCaptureSelected
        {
            get { return _isClipboardCaptureSelected; }
            set 
            { 
                _isClipboardCaptureSelected = value;
                OnPropertyChanged(nameof(IsClipboardCaptureSelected));
            }
        }

        public Timer PeriodicTimerProcessing { get; }
        public Timer PeriodicTimerDownload { get; }
        public Timer PeriodicTimerPinger { get; }
        public Timer PeriodicTimerClipper { get; }
        #endregion

        #region Methods
        public void DownloadButtonClick()
        {
            if (string.IsNullOrWhiteSpace(DownloadLink))
            {
                DownloadLinkDisabler(this);
                StandardOutput = "Empty link";
                return;
            }
            if (!_isDownloadRunning)
            {
                ButtonContent = "Cancel";
                Task.Run(() => ThreadPoolWorker());
            }
            else
            {
                _measureProcessingTime = false;
                _measureDownloadTime = false;
                _downloadedFileSize = null;
                _processingTime = 1;
                _downloadTime = 1;
                _isDownloadRunning = false;
                StandardOutput = "Ready";
                TimersOutput = string.Empty;
                ButtonContent = "Download";
                EnableInteractions();
                _currentThreadPoolWorker.Abort();
            }
        }
        public void HelpButtonClick()
        {
            Process.Start("https://chriskolan.github.io/AudioDownloader/details.html");
        }
        public void DownloadLinkButtonClick()
        {
            Process.Start("https://github.com/ChrisKolan/AudioDownloader/releases/latest/download/AudioDownloader.zip");
        }
        public void FolderButtonClick()
        {
            var pathToExe = Assembly.GetEntryAssembly().Location;
            var pathToExeFolder = System.IO.Path.GetDirectoryName(pathToExe);
            var pathToAudioFolder = pathToExeFolder + @"\audio";

            try
            {
                Process.Start(pathToAudioFolder);
            }
            catch (Exception)
            {
                StandardOutput = "Ready. Audio folder does not exist. Try to download some audio files first.";
            }
        }
        private void ThreadPoolWorker()
        {
            _isDownloadRunning = true;
            string selectedQuality = Helpers.GetQuality(SelectedQuality);
            DisableInteractions();
            Thread.CurrentThread.IsBackground = true;
            _currentThreadPoolWorker = Thread.CurrentThread;
            int positionFrom;
            int positionTo;
            var date = DateTime.Now.ToString("yyMMdd");

            StandardOutput = "Starting download...";
            string command;
            (command, _finishedMessage) = Helpers.CreateCommandAndMessage(selectedQuality, date, DownloadLink);

            var startinfo = new ProcessStartInfo("CMD.exe", command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var process = new Process { StartInfo = startinfo };
            process.Start();

            var reader = process.StandardOutput;
            while (!reader.EndOfStream)
            {
                StandardOutput = reader.ReadLine();
                if (StandardOutput.Contains("[download]") && StandardOutput.Contains("ETA"))
                {
                    _measureDownloadTime = true;
                    positionFrom = StandardOutput.IndexOf("of ") + "of ".Length;
                    positionTo = StandardOutput.LastIndexOf(" at");

                    if ((positionTo - positionFrom) > 0)
                        DownloadedFileSize = StandardOutput.Substring(positionFrom, positionTo - positionFrom);

                    positionFrom = StandardOutput.IndexOf("] ") + "] ".Length;
                    positionTo = StandardOutput.LastIndexOf("%");

                    if ((positionTo - positionFrom) > 0)
                    {
                        var percent = StandardOutput.Substring(positionFrom, positionTo - positionFrom);
                        if (double.TryParse(percent.Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var downloadedPercent))
                        {
                            IsIndeterminate = false;
                            TaskbarItemProgressStateModel = TaskbarItemProgressState.Normal;
                            ProgressBarPercent = Convert.ToInt32(Math.Round(downloadedPercent));
                            TaskBarProgressValue = GetTaskBarProgressValue(100, ProgressBarPercent);
                        }
                        else
                        {
                            IsIndeterminate = true;
                            TaskbarItemProgressStateModel = TaskbarItemProgressState.Indeterminate;
                        }
                    }
                }
                if (StandardOutput.Contains("has already been downloaded"))
                {
                    _downloadedFileSize = "File has already been downloaded.";
                    _measureDownloadTime = false;
                }
                if (StandardOutput.Contains("[ffmpeg]"))
                {
                    StandardOutput = _finishedMessage;
                    _measureProcessingTime = true;
                    _measureDownloadTime = false;
                }
            }

            process.WaitForExit();
            _measureProcessingTime = false;

            if (_downloadedFileSize == null)
            {
                TimersOutput = string.Empty;
                TaskBarProgressValue = GetTaskBarProgressValue(100, 100);
                TaskbarItemProgressStateModel = TaskbarItemProgressState.Error;
                Thread.Sleep(1000);
                if (_isOnline)
                {
                    StandardOutput = "Error. No file downloaded. Updates are needed.";
                }
                else
                {
                    StandardOutput = "Error. No internet connection. No file downloaded.";
                }
                ButtonContent = "Download";
                EnableInteractions();
                _isDownloadRunning = false;
                process.Dispose();
                return;
            }
            if (_downloadedFileSize == "File has already been downloaded.")
            {
                TimersOutput = string.Empty;
                TaskBarProgressValue = GetTaskBarProgressValue(100, 100);
                TaskbarItemProgressStateModel = TaskbarItemProgressState.Paused;
                Thread.Sleep(1000);
                StandardOutput = "Ready. " + _downloadedFileSize;
                ButtonContent = "Download";
                EnableInteractions();
                _isDownloadRunning = false;
                process.Dispose();
                return;
            }
            else
            {
                var processingTimeTimer = (_processingTime * _timerResolution) / 1000.0;
                var downloadTimeTimer = (_downloadTime * _timerResolution) / 1000.0;
                (string fileName, double fileSize) = GetFileNameAndSize(selectedQuality);
                if (_downloadedFileSize.Contains("~"))
                {
                    _downloadedFileSize = _downloadedFileSize.Substring(1);
                }
                TimersOutput = string.Empty;
                StandardOutput = "Done. Processing time: " + processingTimeTimer.ToString("N0") + "s. " +
                                 "Download time: " + downloadTimeTimer.ToString("N0") + "s. " +
                                 "Downloaded file size: " + _downloadedFileSize + ". " +
                                 "Transcoded file size: " + fileSize.ToString("F") + "MiB. ";

                if (double.TryParse(_downloadedFileSize.Remove(_downloadedFileSize.Length - 3), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var downloadedFileSize))
                {
                    var ratio = downloadedFileSize / fileSize;
                    StandardOutput += "Ratio: " + ratio.ToString("F") + ".";
                }

                SendData(fileName, StandardOutput);
                _downloadedFileSize = null;
                _processingTime = 1;
                _downloadTime = 1;
                ButtonContent = "Download";
                EnableInteractions();
                _isDownloadRunning = false;
                process.Dispose();
            }
        }

        private void SendData(string fileName, string standardOutput)
        {
            try
            {
                UpdateWebhook(DownloadLink, fileName, standardOutput);
            }
            catch (Exception)
            {
                TaskBarProgressValue = GetTaskBarProgressValue(100, 100);
                TaskbarItemProgressStateModel = TaskbarItemProgressState.Error;
                Thread.Sleep(1000);
                StandardOutput = "Exception. Updating webhook failed.";
                _downloadedFileSize = null;
                EnableInteractions();
                return;
            }
        }

        private double GetTaskBarProgressValue(int maximum, int progress)
        {
            return (double)progress / (double)maximum;
        }

        private void GetYouTubeAvailableFormatsWorker(Object state)
        {
            IsIndeterminate = true;
            IsComboBoxEnabled = false;
            TaskbarItemProgressStateModel = TaskbarItemProgressState.Indeterminate;
            SynchronizationContext uiContext = state as SynchronizationContext;
            var command = "/C bin\\youtube-dl.exe -F " + DownloadLink;
            var availableFormats = new List<string>();
            var availableAudioFormats = new List<string>();
            availableFormats.Add("\n==========================================================================");
            availableFormats.Add("Advanced information. Available YouTube file formats:");

            var startinfo = new ProcessStartInfo("CMD.exe", command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var process = new Process { StartInfo = startinfo };
            process.Start();

            var reader = process.StandardOutput;
            while (!reader.EndOfStream)
            {
                var currentLine = reader.ReadLine();
                if (currentLine.Contains("["))
                {
                    availableFormats.Add(currentLine);
                    continue;
                }

                availableFormats.Add(ArragementFileFormatsOutput(currentLine));
                
                if (currentLine.Contains("audio only"))
                {
                    availableAudioFormats.Add(currentLine);
                }
            }
            if(uiContext == null)
            {
                return;
            }
            uiContext.Send(UpdateUiFromTheWorkerThread, availableAudioFormats);
            HelpButtonToolTip = LocalVersions + String.Join(Environment.NewLine, availableFormats.ToArray());
            IsIndeterminate = false;
            TaskbarItemProgressStateModel = TaskbarItemProgressState.Normal;
        }

        private void UpdateUiFromTheWorkerThread(object state)
        {
            Quality.Clear();
            var availableAudioFormats = state as List<string>;
            var qualityDynamic = new List<string>();
            var qualityDynamicFormat = new List<string>();
            availableAudioFormats.Reverse();
            bool addOpus = true, addVorbis = true, addM4a = true;

            foreach (var item in availableAudioFormats)
            {
                if (Helpers.FindFormat(item).Contains("opus"))
                {
                    if (addOpus)
                    {
                        qualityDynamic.Add("Audio quality: raw webm \t WebM (Opus) unprocessed");
                        qualityDynamic.Add("Audio quality: raw opus \t Opus unprocessed");
                        addOpus = false;
                    }
                }
                if (Helpers.FindFormat(item).Contains("vorbis"))
                {
                    if (addVorbis)
                    {
                        qualityDynamic.Add("Audio quality: raw vorbis \t Vorbis unprocessed");
                        addVorbis = false;
                    }
                }
                if (Helpers.FindFormat(item).Contains("m4a"))
                {
                    if (addM4a)
                    {
                        qualityDynamic.Add("Audio quality: raw aac \t AAC (m4a) unprocessed");
                        addM4a = false;
                    }
                }

                qualityDynamicFormat.Add(ArragementDynamicFormatsOutput(item));
            }

            qualityDynamic.ForEach(Quality.Add);
            QualityDefault.ForEach(Quality.Add);
            qualityDynamicFormat.ForEach(Quality.Add);
            Quality.Add("Audio and video \t\t Best quality");

            var optimalQualityIndex = Quality.ToList().FindIndex(x => x.Contains("m4a"));
            if(optimalQualityIndex != -1)
                SelectedQuality = Quality[optimalQualityIndex];
            else
            {
                Quality.Clear();
                Quality.Add("Audio quality could not be retrieved.");
                SelectedQuality = Quality[0];
                if (_isOnline)
                {
                    StandardOutput = "YouTube link is invalid";
                }
                else
                {
                    StandardOutput = "Error. No internet connection."; 
                }
                IsButtonEnabled = false;
                return;
            }
            StandardOutput = "Ready";
            IsComboBoxEnabled = true;
        }
        private static string ArragementDynamicFormatsOutput(string currentLine)
        {
            string result;
            var stringCleanedFromSpaces = System.Text.RegularExpressions.Regex.Replace(currentLine, @"\s+", " ");
            var index = stringCleanedFromSpaces.IndexOf(' ', 0);
            var replaceFirstSpace = stringCleanedFromSpaces.Insert(index, "\t").Remove(index + 1, 1);

            var index2 = replaceFirstSpace.IndexOf(' ', index);
            result = replaceFirstSpace.Insert(index2, "\t\t\t").Remove(index2 + 2, 1);
            return result;
        }

        private static string ArragementFileFormatsOutput(string currentLine)
        {
            string result;

            if (currentLine.Contains("format code"))
            {
                result = "format code\textension\tresolution note";
                return result;
            }
            
            var stringCleanedFromSpaces = System.Text.RegularExpressions.Regex.Replace(currentLine, @"\s+", " ");
            var index = stringCleanedFromSpaces.IndexOf(' ', 0);
            var replaceFirstSpace = stringCleanedFromSpaces.Insert(index, "\t\t").Remove(index + 2, 1);

            var index2 = replaceFirstSpace.IndexOf(' ', index);
            result = replaceFirstSpace.Insert(index2, "\t\t").Remove(index2 + 2, 1);
            return result;
        }

        public void GetLocalVersions()
        {
            var localVersionsNamesAndNumber = new List<string>();
            localVersionsNamesAndNumber.Add("Press button to get online help.");
            localVersionsNamesAndNumber.Add("===========================");
            localVersionsNamesAndNumber.Add("Software \t   |\tVersion");
            localVersionsNamesAndNumber.Add("----------------------|-----------------------");
            var localVersions = LocalVersionProvider.Versions();
            var localVersionsSoftwareNames = new List<string>
            {
                "Audio Downloader  |\t",
                "Youtube-dl\t   |\t"
            };

            for (int i = 0; i < localVersions.Count; i++)
            {
                localVersionsNamesAndNumber.Add(localVersionsSoftwareNames[i] + localVersions[i]);
            }

            localVersionsNamesAndNumber.Add("FFmpeg\t\t   |\t4.2.1");

            LocalVersions = String.Join(Environment.NewLine, localVersionsNamesAndNumber.ToArray());
            HelpButtonToolTip = LocalVersions;
        }

        public void EnableInteractions()
        {
            IsIndeterminate = false;
            IsInputEnabled = true;
            IsButtonEnabled = true;
            IsComboBoxEnabled = true;
            ProgressBarPercent = 0;
            TaskBarProgressValue = GetTaskBarProgressValue(100, ProgressBarPercent);
            TaskbarItemProgressStateModel = TaskbarItemProgressState.Normal;
        }

        public void DisableInteractions()
        {
            IsIndeterminate = true;
            IsInputEnabled = false;
            IsComboBoxEnabled = false;
            ProgressBarPercent = 0;
            _downloadFileSize = 0.0;
            _audioVideoDownloadCounter = 0;
            TaskBarProgressValue = GetTaskBarProgressValue(100, ProgressBarPercent);
            TaskbarItemProgressStateModel = TaskbarItemProgressState.Indeterminate;
        }

        private void ProcessingTimeMeasurement()
        {
            if (!_measureProcessingTime)
            {
                return;
            }
            _processingTime++;
            IsIndeterminate = true;
            TimersOutput = "Processing time: " + ((_processingTime * _timerResolution) / 1000.0).ToString("N1") + "s";
            TaskbarItemProgressStateModel = TaskbarItemProgressState.Indeterminate;
        }

        private void DownloadTimeMeasurement()
        {
            if (!_measureDownloadTime)
            {
                return;
            }
            if (_isOnline)
            {
                _downloadTime++;
                TimersOutput = "Download time: " + ((_downloadTime * _timerResolution) / 1000.0).ToString("N1") + "s";
            }
        }

        private void TimerClipper()
        {
            if (IsClipboardCaptureSelected)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (DownloadLink == null)
                    {
                        DownloadLink = Clipboard.GetText();
                    }
                    if (!DownloadLink.Contains(Clipboard.GetText()))
                    {
                        DownloadLink = Clipboard.GetText();
                    }
                }, System.Windows.Threading.DispatcherPriority.Send);
            }
        }

        private void TimerPinger()
        {
            if (Helpers.Pinger())
            {
                _isOnline = true;
                if (TimersOutput != null && TimersOutput.Contains("Offline"))
                {
                    TimersOutput = string.Empty;
                }
                if (StandardOutput.Contains("Error. No internet connection."))
                {
                    StandardOutput = "Internet connection reestablished.";
                }
                if (Application.Current == null)
                {
                    return;
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GlowBrushColor = new SolidColorBrush(Colors.LightBlue);
                });
                _pingerCounter = 0;
            }
            else
            {
                if (_pingerCounter > 2)
                {
                    _isOnline = false;
                    TimersOutput = "Offline";
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        GlowBrushColor = new SolidColorBrush(Colors.Red);
                    });
                }
                _pingerCounter++;
            }
        }

        private string Turn()
        {
            _counter++;
            switch (_counter % 4)
            {
                case 0: StandardOutput = _finishedMessage + "."; break;
                case 1: StandardOutput = _finishedMessage + ".."; break;
                case 2: StandardOutput = _finishedMessage + "..."; break;
                case 3: StandardOutput = _finishedMessage + "...."; break;
            }

            return StandardOutput;
        }

        private void UpdateWebhook(string youtubeLink, string fileName, string standardOutput)
        {
            string eventName = ConfigurationManager.AppSettings["event"];
            string secretKey = ConfigurationManager.AppSettings["writeKey"];
            if (string.IsNullOrWhiteSpace(eventName) || string.IsNullOrWhiteSpace(secretKey))
            {
                return;
            }
            var pcName = Environment.MachineName;
            var values = new Dictionary<string, string>();
            values.Add("value1", pcName + " " + standardOutput.Substring(6));
            values.Add("value2", fileName);
            values.Add("value3", youtubeLink);
            WebhookTrigger.SendRequestAsync(values, eventName, secretKey);
        }

        private (string fileName, double fileSize) GetFileNameAndSize(string selectedQuality)
        {
            string path = selectedQuality.Contains("video") ? "audio\\video\\" : "audio\\";
            var directory = new DirectoryInfo(path);
            var myFile = directory.GetFiles()
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();

            var length = new FileInfo(path + myFile.ToString()).Length;

            return (fileName: myFile.ToString(), fileSize: ConvertBytesToMegabytes(length));
        }

        private static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        private void DownloadLinkDisabler(Model model)
        {
            model.DownloadLinkEnabled = false;
            model.DownloadLinkTextDecorations = null;
        }

        #endregion

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region INotifyDataErrorInfo implementation

        private ConcurrentDictionary<string, List<string>> _errors = new ConcurrentDictionary<string, List<string>>();

        public static ValidationResult ValidateDownloadLink(object obj, ValidationContext context)
        {
            var model = (Model)context.ObjectInstance;

            if (!model.DownloadLink.Contains("https://www.youtube.com/"))
            {
                model.DownloadLinkDisabler(model);
                model.IsButtonEnabled = false;
                model.IsComboBoxEnabled = false;
                return new ValidationResult("YouTube link not valid", new List<string> { "DownloadLink" });
            }
            model.DownloadLinkDisabler(model);
            model.IsButtonEnabled = true;
            model.IsComboBoxEnabled = true;
            var uiContext = model._synchronizationContext;
            model.StandardOutput = "Retrieving audio quality";
            Task.Run(() => model.GetYouTubeAvailableFormatsWorker(uiContext));

            return ValidationResult.Success;
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            List<string> errorsForName;
            _errors.TryGetValue(propertyName, out errorsForName);
            return errorsForName;
        }

        public bool HasErrors
        {
            get { return _errors.Any(kv => kv.Value != null && kv.Value.Count > 0); }
        }

        public Task ValidateAsync()
        {
            return Task.Run(() => Validate());
        }

        private object _lock = new object();
        public void Validate()
        {
            lock (_lock)
            {
                var validationContext = new ValidationContext(this, null, null);
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateObject(this, validationContext, validationResults, true);

                foreach (var kv in _errors.ToList())
                {
                    if (validationResults.All(r => r.MemberNames.All(m => m != kv.Key)))
                    {
                        List<string> outLi;
                        _errors.TryRemove(kv.Key, out outLi);
                        OnErrorsChanged(kv.Key);
                    }
                }

                var q = from r in validationResults
                        from m in r.MemberNames
                        group r by m into g
                        select g;

                foreach (var prop in q)
                {
                    var messages = prop.Select(r => r.ErrorMessage).ToList();

                    if (_errors.ContainsKey(prop.Key))
                    {
                        List<string> outLi;
                        _errors.TryRemove(prop.Key, out outLi);
                    }
                    _errors.TryAdd(prop.Key, messages);
                    OnErrorsChanged(prop.Key);
                }
            }
        }

        #endregion
    }
}