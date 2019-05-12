using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Voltsoft.FileVersionUtilities;

namespace ViewModel
{
    public class ViewModel : INotifyPropertyChanged
    {
        private string _selectedQuality;

        public ViewModel()
        {
            Model = new Model.Model();
            var updaterTask = YoutubeDlUpdater();
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
            DownloadButton = new Helper.ActionCommand(DownloadButtonCommand);
        }

        public Model.Model Model { get; set; }
        public Helper.ActionCommand DownloadButton { get; set; }
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

        private async Task YoutubeDlUpdater()
        {
            Model.StandardOutput = "Checking if new version is available.";
            Model.DisableInteractions();

            try
            {
                var pathToExeFolder = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string pathToYoutubeDl = pathToExeFolder + @"\bin\youtube-dl.exe";
                var currentYoutubeDlVersion = Utilities.GetFileVersion(pathToYoutubeDl);

                var client = new GitHubClient(new ProductHeaderValue("audio-downloader"));
                client.SetRequestTimeout(TimeSpan.FromSeconds(20));
                var releases = await client.Repository.Release.GetLatest("ytdl-org", "youtube-dl");
                var newYoutubeDlVersion = releases.TagName;
                if (currentYoutubeDlVersion == null || currentYoutubeDlVersion != newYoutubeDlVersion)
                {
                    Model.StandardOutput = "Downloading new Youtube-dl version. Current version: " + currentYoutubeDlVersion + ". New version: " + newYoutubeDlVersion;
                    Model.IsIndeterminate = true;
                    var latestAsset = await client.Repository.Release.GetAllAssets("ytdl-org", "youtube-dl", releases.Id);
                    var latestUri = latestAsset[7].BrowserDownloadUrl;
                    var response = await client.Connection.Get<object>(new Uri(latestUri), new Dictionary<string, string>(), "application/octet-stream");
                    var responseData = response.HttpResponse.Body;
                    System.IO.File.WriteAllBytes(pathToYoutubeDl, (byte[])responseData);
                    Model.StandardOutput = "Updated Youtube-dl to version: " + newYoutubeDlVersion + ". Status: idle";
                }
                else
                {
                    Model.StandardOutput = "Status: idle";
                }
            }
            catch (Exception)
            {
                Model.StandardOutput = "Exception during update. Status: idle";
            }

            Model.EnableInteractions();
        }

        private void DownloadButtonCommand()
        {
            if (string.IsNullOrWhiteSpace(DownloadLink))
            {
                Model.StandardOutput = "Empty link";
                return;
            }
           
            Model.DownloadButtonClick(DownloadLink, GetQuality());
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

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
