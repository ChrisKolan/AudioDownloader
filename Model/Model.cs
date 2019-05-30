using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Webhook;

namespace Model
{
    public class Model : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Fields
        private string _standardOutput;
        private static bool _isSpinning;
        private int _counter;
        private string _finishedMessage;
        private string _downloadedFileSize;
        private int _progressBarPercent;
        private bool _isIndeterminate;
        private bool _isButtonEnabled;
        private bool _isInputEnabled;
        private string _selectedQuality;
        private bool _isWaitingForData;
        private string _helpButtonToolTip;
        private string _downloadLink;
        private bool _isComboBoxEnabled;
        private int _processingTime = 1;
        private int _timerResolution = 10;
        private SynchronizationContext _synchronizationContext;
        #endregion

        #region Constructor
        public Model()
        {
            StandardOutput = "Status: idle";
            EnableInteractions();
            PeriodicTimer = new Timer(_ => Spinner(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_timerResolution));

            QualityDefault = new List<string>
            {
                "Audio quality: superb. \t FLAC lossless compression (Largest flac file size).",
                "Audio quality: best. \t Bitrate average: 245 kbit/s, Bitrate range: 220-260 kbit/s (Large mp3 file size).",
                "Audio quality: better. \t Bitrate average: 225 kbit/s, Bitrate range: 190-250 kbit/s. VBR mp3 lossy compression.",
                "Audio quality: optimal. \t Bitrate average: 190 kbit/s, Bitrate range: 170-210 kbit/s. VBR mp3 lossy compression.",
                "Audio quality: very good. \t Bitrate average: 175 kbit/s, Bitrate range: 150-195 kbit/s. VBR mp3 lossy compression.",
                "Audio quality: transparent. \t Bitrate average: 165 kbit/s, Bitrate range: 140-185 kbit/s. VBR mp3 lossy compression (Balanced mp3 file size).",
                "Audio quality: good. \t Bitrate average: 130 kbit/s, Bitrate range: 120-150 kbit/s. VBR mp3 lossy compression.",
                "Audio quality: acceptable. \t Bitrate average: 115 kbit/s, Bitrate range: 100-130 kbit/s. VBR mp3 lossy compression.",
                "Audio quality: audio book. \t Bitrate average: 100 kbit/s, Bitrate range: 080-120 kbit/s. VBR mp3 lossy compression.",
                "Audio quality: worse. \t Bitrate average: 085 kbit/s, Bitrate range: 070-105 kbit/s. VBR mp3 lossy compression.",
                "Audio quality: worst. \t Bitrate average: 065 kbit/s, Bitrate range: 045-085 kbit/s. VBR mp3 lossy compression (Smallest mp3 file size)."
            };
            Quality = new ObservableCollection<string>();
            SelectedQuality = QualityDefault[7];
            _ = ApplicationUpdater.UpdateAsync(this);
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

        public bool IsComboBoxEnabled
        {
            get { return _isComboBoxEnabled; }
            set
            {
                _isComboBoxEnabled = value;
                OnPropertyChanged(nameof(IsComboBoxEnabled));
            }
        }

        public string SelectedQuality
        {
            get { return _selectedQuality; }
            set
            {
                _selectedQuality = value;
                OnPropertyChanged(nameof(SelectedQuality));
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

        public bool IsWaitingForData
        {
            get { return _isWaitingForData; }
            set
            {
                _isWaitingForData = value;
                OnPropertyChanged(nameof(IsWaitingForData));
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

        public Timer PeriodicTimer { get; }
        #endregion

        #region Methods
        public void DownloadButtonClick()
        {
            if (string.IsNullOrWhiteSpace(DownloadLink))
            {
                StandardOutput = "Empty link";
                return;
            }

            ThreadPool.QueueUserWorkItem(ThreadPoolWorker);
        }
        public void HelpButtonClick()
        {
            Process.Start("https://chriskolan.github.io/AudioDownloader/details.html");
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
            catch (Exception exception)
            {
                StandardOutput = "Status: idle. Audio folder does not exist. Try to download some audio files first.\n" + exception.ToString();
            }
        }
        private void ThreadPoolWorker(Object stateInfo)
        {
            var watch = Stopwatch.StartNew();
            string selectedQuality = GetQuality();
            DisableInteractions();
            long elapsedTimeInMiliseconds;
            Thread.CurrentThread.IsBackground = true;
            int positionFrom;
            int positionTo;

            if (DownloadLink.Contains("CLI"))
            {
                StandardOutput = "Advanced mode. Use on your own risk. Starting download in a new command window. Close the window to start new download.";
                var advancedUserCommand = DownloadLink.Remove(0, 4);
                /// "/K" keeps command window open
                var command = "/K bin\\youtube-dl.exe " + advancedUserCommand;

                try
                {
                    Process process = Process.Start("CMD.exe", command);
                    process.WaitForExit();
                }
                catch
                {
                    StandardOutput = "Exception. Processed command: " + command;
                }

                EnableInteractions();
            }
            else
            {
                if (!DownloadLink.Contains("https://www.youtube.com/watch?v="))
                {
                    StandardOutput = "YouTube link not valid";
                    EnableInteractions();
                    return;
                }

                StandardOutput = "Starting download...";
                //GetLocalVersions();
                //GetYouTubeAvailableFormats(DownloadLink);
                string command;
                var date = DateTime.Now.ToString("yyMMdd");

                if (selectedQuality.Contains("mp3"))
                {
                    var quality = GetQuality(selectedQuality);
                    command = "/C bin\\youtube-dl.exe -f bestaudio[ext=webm] --extract-audio --audio-format mp3 --no-mtime --add-metadata --audio-quality " + quality + " --restrict-filenames -o audio\\" + date + "Q" + quality + "-%(title)s-%(id)s.%(ext)s " + DownloadLink;
                    _finishedMessage = "Status: download finished. Now transcoding to mp3. This may take a while. Processing.";
                }
                else if (selectedQuality.Contains("flac"))
                {
                    command = "/C bin\\youtube-dl.exe -f bestaudio[ext=webm] --extract-audio --audio-format flac --no-mtime --add-metadata --restrict-filenames -o audio\\" + date + "-%(title)s-%(id)s.%(ext)s " + DownloadLink;
                    _finishedMessage = "Status: download finished. Now transcoding to FLAC. This may take a while. Processing.";
                }
                else if (selectedQuality.Contains("raw webm"))
                {
                    command = "/C bin\\youtube-dl.exe -f bestaudio[ext=webm] --no-mtime --add-metadata --restrict-filenames -o audio\\" + date + "-%(title)s-%(id)s.%(ext)s " + DownloadLink;
                    _finishedMessage = "Status: download finished.";
                }
                else if (selectedQuality.Contains("raw opus"))
                {
                    command = "/C bin\\youtube-dl.exe --extract-audio --format bestaudio[acodec=opus] --no-mtime --add-metadata --restrict-filenames -o audio\\" + date + "-%(title)s-%(id)s.%(ext)s " + DownloadLink;
                    _finishedMessage = "Status: download finished.";
                }
                else if (selectedQuality.Contains("raw aac"))
                {
                    command = "/C bin\\youtube-dl.exe -f bestaudio[ext=m4a] --no-mtime --add-metadata --restrict-filenames -o audio\\" + date + "-%(title)s-%(id)s.%(ext)s " + DownloadLink;
                    _finishedMessage = "Status: download finished.";
                }
                else if (selectedQuality.Split(' ').First().All(char.IsDigit))
                {
                    var formatCode = selectedQuality.Split(' ').First();
                    var format = selectedQuality.Split(' ').Last();
                    command = "/C bin\\youtube-dl.exe -f " + formatCode + " --extract-audio  --audio-format " + format + " --no-mtime --add-metadata --restrict-filenames -o audio\\" + date + "-%(title)s-%(id)s.%(ext)s " + DownloadLink;
                    _finishedMessage = "Status: download finished.";
                }
                else 
                {
                    command = "/C bin\\youtube-dl.exe --extract-audio --format bestaudio[acodec=vorbis] --no-mtime --add-metadata --restrict-filenames -o audio\\" + date + "-%(title)s-%(id)s.%(ext)s " + DownloadLink;
                    _finishedMessage = "Status: download finished.";
                }

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
                        IsIndeterminate = false;
                        positionFrom = StandardOutput.IndexOf("of ") + "of ".Length;
                        positionTo = StandardOutput.LastIndexOf(" at");

                        if ((positionTo - positionFrom) > 0)
                            _downloadedFileSize = StandardOutput.Substring(positionFrom, positionTo - positionFrom);

                        positionFrom = StandardOutput.IndexOf("] ") + "] ".Length;
                        positionTo = StandardOutput.LastIndexOf("%");

                        if ((positionTo - positionFrom) > 0)
                        {
                            var percent = StandardOutput.Substring(positionFrom, positionTo - positionFrom);
                            ProgressBarPercent = Convert.ToInt32(Math.Round(Convert.ToDouble(percent))); ;
                        }
                    }
                    if (StandardOutput.Contains("has already been downloaded"))
                    {
                        _downloadedFileSize = "File has already been downloaded. ";
                    }
                    if (StandardOutput.Contains("[ffmpeg]"))
                    {
                        StandardOutput = _finishedMessage;
                        _isSpinning = true;
                    }
                }

                process.WaitForExit();
                _isSpinning = false;

                if (_downloadedFileSize == null)
                {
                    watch.Stop();
                    elapsedTimeInMiliseconds = watch.ElapsedMilliseconds;
                    StandardOutput = "Status: error. Downloaded file size is zero. Check whether the selected format exists. Elapsed time: " + elapsedTimeInMiliseconds + "ms. ";
                    EnableInteractions();
                    return;
                }
                if (_downloadedFileSize == "File has already been downloaded. ")
                {
                    watch.Stop();
                    elapsedTimeInMiliseconds = watch.ElapsedMilliseconds;
                    StandardOutput = "Status: idle. " + _downloadedFileSize + "Elapsed time: " + elapsedTimeInMiliseconds + "ms. ";
                    EnableInteractions();
                    return;
                }
                else
                {
                    (string fileName, double fileSize) = GetFileNameAndSize();

                    try
                    {
                        UpdateWebhook(DownloadLink, fileName);
                    }
                    catch (Exception)
                    {
                        watch.Stop();
                        elapsedTimeInMiliseconds = watch.ElapsedMilliseconds;
                        StandardOutput = "Status: exception. Updating webhook failed. Elapsed time: " + elapsedTimeInMiliseconds + "ms. ";
                        _downloadedFileSize = null;
                        EnableInteractions();
                        return;
                    }

                    var downloadedFileSize = double.Parse(_downloadedFileSize.Remove(_downloadedFileSize.Length - 3));
                    var ratio = downloadedFileSize / fileSize;
                    watch.Stop();
                    elapsedTimeInMiliseconds = watch.ElapsedMilliseconds;
                    var processingTimeTimer = _processingTime * _timerResolution;

                    StandardOutput = "Status: done. Processing time: " + processingTimeTimer + "ms. " + 
                                     "Elapsed time: " + elapsedTimeInMiliseconds + "ms. " +
                                     "Downloaded file size: " + _downloadedFileSize + ". " +
                                     "Transcoded file size: " + fileSize.ToString("F") + "MiB. " +
                                     "Ratio: " + ratio.ToString("F") + ".";
                    _downloadedFileSize = null;
                    EnableInteractions();
                }
            }
        }

        private void GetYouTubeAvailableFormatsWorker(Object state)
        {
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

            //uiContext.Send(x => availableAudioFormats.ForEach(Quality.Add), null);
            uiContext.Send(UpdateUiFromTheWorkerThread, availableAudioFormats);

            HelpButtonToolTip = LocalVersions + String.Join(Environment.NewLine, availableFormats.ToArray());
        }

        private void UpdateUiFromTheWorkerThread(object state)
        {
            Quality.Clear();
            var availableAudioFormats = state as List<string>;
            var qualityDynamic = new List<string>();
            var qualityDynamicFormat = new List<string>();
            availableAudioFormats.Reverse();
            qualityDynamic.Clear();
            qualityDynamicFormat.Clear();

            foreach (var item in availableAudioFormats)
            {
                if (FindFormat(item).Contains("opus"))
                {
                    qualityDynamic.Insert(0, "Audio quality: raw webm. \t WebM (Opus) unprocessed.");
                    qualityDynamic.Insert(0, "Audio quality: raw opus. \t Opus unprocessed.");
                }
                if (FindFormat(item).Contains("vorbis"))
                {
                    qualityDynamic.Insert(0, "Audio quality: raw vorbis. \t Vorbis unprocessed.");
                }
                if (FindFormat(item).Contains("m4a"))
                {
                    qualityDynamic.Insert(0, "Audio quality: raw aac. \t AAC(m4a) unprocessed.");
                }

                qualityDynamicFormat.Add(item);
            }

            qualityDynamic.ForEach(Quality.Add);
            QualityDefault.ForEach(Quality.Add);
            qualityDynamicFormat.ForEach(Quality.Add);
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

            localVersionsNamesAndNumber.Add("FFmpeg\t\t   |\t4.1.3");

            LocalVersions = String.Join(Environment.NewLine, localVersionsNamesAndNumber.ToArray());
            HelpButtonToolTip = LocalVersions;
        }

        public void EnableInteractions()
        {
            IsIndeterminate = false;
            IsInputEnabled = true;
            IsButtonEnabled = true;
            IsWaitingForData = true;
            IsComboBoxEnabled = true;
        }

        public void DisableInteractions()
        {
            IsIndeterminate = true;
            IsInputEnabled = false;
            IsButtonEnabled = false;
            IsWaitingForData = false;
            IsComboBoxEnabled = false;
            ProgressBarPercent = 0;
        }

        private void Spinner()
        {
            if (!_isSpinning)
            {
                _processingTime = 1;
                return;
            }
            _processingTime++;
            IsIndeterminate = true;
            //StandardOutput = Turn();
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

        private string GetQuality(string selectedQuality)
        {
            string[] qualityArray = selectedQuality.Split(' ');

            return qualityArray[1];
        }

        private void UpdateWebhook(string youtubeLink, string fileName)
        {
            var pcName = Environment.MachineName;
            string eventName = ConfigurationManager.AppSettings["event"];
            string secretKey = ConfigurationManager.AppSettings["writeKey"];
            var values = new Dictionary<string, string>();

            values.Add("value1", pcName);
            values.Add("value2", fileName);
            values.Add("value3", youtubeLink);

            if (string.IsNullOrWhiteSpace(eventName) || string.IsNullOrWhiteSpace(secretKey))
                return;

            WebhookTrigger.SendRequestAsync(values, eventName, secretKey);
        }

        private (string fileName, double fileSize) GetFileNameAndSize()
        {
            string audioDirectory = "audio\\";
            var directory = new DirectoryInfo(audioDirectory);
            var myFile = directory.GetFiles()
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();

            var length = new FileInfo(audioDirectory + myFile.ToString()).Length;

            return (fileName: myFile.ToString(), fileSize: ConvertBytesToMegabytes(length));
        }

        private static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        private string GetQuality()
        {
            if (SelectedQuality.Contains("raw webm"))
                return "raw webm";
            else if (SelectedQuality.Contains("raw opus"))
                return "raw opus";
            else if (SelectedQuality.Contains("raw aac"))
                return "raw aac";
            else if (SelectedQuality.Contains("raw vorbis"))
                return "raw vorbis";
            else if (SelectedQuality.Contains("superb"))
                return "flac";
            else if (SelectedQuality.Contains("best"))
                return "mp3 0";
            else if (SelectedQuality.Contains("better"))
                return "mp3 1";
            else if (SelectedQuality.Contains("optimal"))
                return "mp3 2";
            else if (SelectedQuality.Contains("very good"))
                return "mp3 3";
            else if (SelectedQuality.Contains("transparent"))
                return "mp3 4";
            else if (SelectedQuality.Contains("good"))
                return "mp3 5";
            else if (SelectedQuality.Contains("acceptable"))
                return "mp3 6";
            else if (SelectedQuality.Contains("audio book"))
                return "mp3 7";
            else if (SelectedQuality.Contains("worse"))
                return "mp3 8";
            else if (SelectedQuality.Contains("worst"))
                return "mp3 9";
            else if (SelectedQuality.Split(' ').First().All(char.IsDigit))
            {
                var format = FindFormat(SelectedQuality);
                var formatCode = SelectedQuality.Split(' ').First();
                return formatCode + " " + format;
            }
            else
                return "mp3 4";
        }

        private string FindFormat(string selectedQuality)
        {
            if (selectedQuality.Contains("m4a"))
                return "m4a";
            else
            {
                if (selectedQuality.Contains("opus"))
                    return "opus";
                else
                    return "vorbis";
            }
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

            if (model.DownloadLink.Contains("CLI"))
            {
                model.IsButtonEnabled = true;
                model.IsComboBoxEnabled = false;
                return ValidationResult.Success;
            }
            if (!model.DownloadLink.Contains("https://www.youtube.com/watch?v="))
            {
                model.IsButtonEnabled = false;
                model.IsComboBoxEnabled = false;
                return new ValidationResult("YouTube link not valid", new List<string> { "DownloadLink" });
            }
            if (model.DownloadLink.Length != 43)
            {
                model.IsButtonEnabled = false;
                model.IsComboBoxEnabled = false;
                return new ValidationResult("YouTube link length not correct", new List<string> { "DownloadLink" });
            }
            model.IsButtonEnabled = true;
            model.IsComboBoxEnabled = true;
            var uiContext = model._synchronizationContext;
            ThreadPool.QueueUserWorkItem(model.GetYouTubeAvailableFormatsWorker, uiContext);

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