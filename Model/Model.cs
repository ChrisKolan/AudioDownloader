using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Webhook;

namespace Model
{
    public class Model : INotifyPropertyChanged
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
        #endregion

        #region Constructor
        public Model()
        {
            StandardOutput = "Status: idle";
            EnableInteractions();
            PeriodicTimer = new Timer(_ => Spinner(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));

            Quality = new ObservableCollection<string>
            {
                "Audio quality: raw webm. \t WebM (Opus) unprocessed.",
                "Audio quality: raw opus. \t Opus unprocessed.",
                "Audio quality: raw vorbis. \t Vorbis unprocessed.",
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
            SelectedQuality = Quality[6];
            _ = ApplicationUpdater.UpdateAsync(this);
        }
        #endregion

        #region Properties
        public string DownloadLink { get; set; }

        public ObservableCollection<string> Quality { get; set; }

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
            Process.Start("https://chriskolan.github.io/AudioDownloader/");
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
                GetYouTubeAvailableFormats(DownloadLink);
                string command;
                var date = DateTime.Now.ToString("yyMMdd");

                if (selectedQuality.Contains("mp3"))
                {
                    var quality = GetQuality(selectedQuality);
                    command = "/C bin\\youtube-dl.exe -f bestaudio[ext=webm] --extract-audio --audio-format mp3 --no-mtime --add-metadata --audio-quality " + quality + " --restrict-filenames -o audio\\" + date + "Q" + quality + "-%(title)s-%(id)s.%(ext)s " + DownloadLink;
                    _finishedMessage = "Download finished. Now converting to mp3. This may take a while. Processing";
                }
                else if (selectedQuality.Contains("flac"))
                {
                    command = "/C bin\\youtube-dl.exe -f bestaudio[ext=webm] --extract-audio --audio-format flac --no-mtime --add-metadata --restrict-filenames -o audio\\" + date + "-%(title)s-%(id)s.%(ext)s " + DownloadLink;
                    _finishedMessage = "Download finished. Now converting to FLAC. This may take a while. Processing";
                }
                else if (selectedQuality.Contains("raw webm"))
                {
                    command = "/C bin\\youtube-dl.exe -f bestaudio[ext=webm] --no-mtime --add-metadata --restrict-filenames -o audio\\" + date + "-%(title)s-%(id)s.%(ext)s " + DownloadLink;
                    _finishedMessage = "Download finished.";
                }
                else if (selectedQuality.Contains("raw opus"))
                {
                    command = "/C bin\\youtube-dl.exe --extract-audio --audio-format opus --no-mtime --add-metadata --restrict-filenames -o audio\\" + date + "-%(title)s-%(id)s.%(ext)s " + DownloadLink;
                    _finishedMessage = "Download finished.";
                }
                else 
                {
                    command = "/C bin\\youtube-dl.exe --extract-audio --audio-format vorbis --no-mtime --add-metadata --restrict-filenames -o audio\\" + date + "-%(title)s-%(id)s.%(ext)s " + DownloadLink;
                    _finishedMessage = "Download finished.";
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
                    StandardOutput = "Error. Elapsed time: " + elapsedTimeInMiliseconds + "ms. ";
                    EnableInteractions();
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
                        StandardOutput = "Exception. Elapsed time: " + elapsedTimeInMiliseconds + "ms. ";
                        _downloadedFileSize = null;
                        EnableInteractions();
                        return;
                    }

                    var downloadedFileSize = double.Parse(_downloadedFileSize.Remove(_downloadedFileSize.Length - 3));
                    var ratio = downloadedFileSize / fileSize;
                    watch.Stop();
                    elapsedTimeInMiliseconds = watch.ElapsedMilliseconds;

                    StandardOutput = "Done. Elapsed time: " + elapsedTimeInMiliseconds + "ms. " +
                                     "Downloaded file size: " + _downloadedFileSize + ". " +
                                     "File size: " + fileSize.ToString("F") + "MiB. " +
                                     "Ratio (downloaded file size)/(file size): " + ratio.ToString("F") + ".";
                    _downloadedFileSize = null;
                    EnableInteractions();
                }
            }
        }

        private void GetYouTubeAvailableFormats(string downloadLink)
        {
            var command = "/C bin\\youtube-dl.exe -F " + downloadLink;
            var availableFormats = new List<string>();
            availableFormats.Add("Press to get online help.");
            availableFormats.Add("==================================================================");
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
                availableFormats.Add(reader.ReadLine());
            }

            HelpButtonToolTip = String.Join(Environment.NewLine, availableFormats.ToArray());
        }
        public void EnableInteractions()
        {
            IsIndeterminate = false;
            IsInputEnabled = true;
            IsButtonEnabled = true;
            IsWaitingForData = true;
        }

        public void DisableInteractions()
        {
            IsIndeterminate = true;
            IsInputEnabled = false;
            IsButtonEnabled = false;
            IsWaitingForData = false;
            ProgressBarPercent = 0;
        }

        private void Spinner()
        {
            if (!_isSpinning)
                return;

            IsIndeterminate = true;
            StandardOutput = Turn();
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
            else
                return "mp3 4";
        }

        #endregion

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}