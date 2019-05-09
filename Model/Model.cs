using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Webhook;

namespace Model
{
    public class Model : INotifyPropertyChanged
    {
        #region Fields
        private string _standardOutput;
        private static bool _isSpinning;
        private int _counter;
        private string _finishedMessage = "Download finished. Now converting to mp3. This may take a while. Processing";
        private string _downloadedFileSize;
        private int _progressBarPercent;
        private bool _isIndeterminate;
        private bool _isButtonEnabled;
        private bool _isInputEnabled;
        #endregion

        #region Constructor
        public Model()
        {
            StandardOutput = "Status: idle";
            EnableInteractions();
            PeriodicTimer = new Timer(_ => Spinner(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
        } 
        #endregion

        #region Properties
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

        public Timer PeriodicTimer { get; }
        #endregion

        #region Methods
        public void DownloadButtonClick(string youtubeLink, string selectedQuality)
        {
            ThreadPool.QueueUserWorkItem(ThreadPoolWorker, new object[] { youtubeLink, selectedQuality });
        }

        private void ThreadPoolWorker(Object stateInfo)
        {
            var watch = Stopwatch.StartNew();
            object[] array = stateInfo as object[];
            string youtubeLink = Convert.ToString(array[0]);
            string selectedQuality = Convert.ToString(array[1]);
            DisableInteractions();
            long elapsedTimeInMiliseconds;
            Thread.CurrentThread.IsBackground = true;
            int positionFrom;
            int positionTo;

            if (youtubeLink.Contains("CLI"))
            {
                StandardOutput = "Advanced mode. Use on your own risk. Starting download in a new command window. Close the window to start new download.";
                var advancedUserCommand = youtubeLink.Remove(0, 4);
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
                if (!youtubeLink.Contains("https://www.youtube.com/watch?v="))
                {
                    StandardOutput = "YouTube link not valid";
                    EnableInteractions();
                    return;
                }

                StandardOutput = "Starting download...";
                string command;
                var date = DateTime.Now.ToString("yyMMdd");
                var quality = GetQuality(selectedQuality);
                string keepDownloadedFiles = ConfigurationManager.AppSettings["keepDownloadedFiles"];

                command = "/C bin\\youtube-dl.exe --extract-audio --audio-format mp3 --no-mtime --audio-quality " + quality + " " + keepDownloadedFiles + " --restrict-filenames -o mp3\\" + date + "Q" + quality + "-%(title)s-%(id)s.%(ext)s " + youtubeLink;

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
                        UpdateWebhook(youtubeLink, fileName);
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
                                     "Mp3 file size: " + fileSize.ToString("F") + "MiB. " +
                                     "Ratio (downloaded size)/(mp3 size): " + ratio.ToString("F") + ".";
                    _downloadedFileSize = null;
                    EnableInteractions();
                }
            }
        }

        private void EnableInteractions()
        {
            IsIndeterminate = false;
            IsInputEnabled = true;
            IsButtonEnabled = true;
        }

        private void DisableInteractions()
        {
            IsIndeterminate = true;
            IsInputEnabled = false;
            IsButtonEnabled = false;
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

            return qualityArray[2];
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
            string mp3Directory = "mp3\\";
            var directory = new DirectoryInfo(mp3Directory);
            var myFile = directory.GetFiles()
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();

            var length = new FileInfo(mp3Directory + myFile.ToString()).Length;

            return (fileName: myFile.ToString(), fileSize: ConvertBytesToMegabytes(length));
        }

        private static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
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