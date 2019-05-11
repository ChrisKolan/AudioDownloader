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
                "Audio quality: 0 Best (Largest mp3 file size)",
                "Audio quality: 1 Better",
                "Audio quality: 2 Optimal",
                "Audio quality: 3 Very good",
                "Audio quality: 4 Good (Balanced mp3 file size)",
                "Audio quality: 5 Default",
                "Audio quality: 6 Average",
                "Audio quality: 7 AudioBook",
                "Audio quality: 8 Worse",
                "Audio quality: 9 Worst (Smallest mp3 file size)"
            };
            SelectedQuality = Quality[2];
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

            try
            {
                var pathToExeFolder = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string pathToYoutubeDl = pathToExeFolder + @"\bin\youtube-dl.exe";
                var currentYoutubeDlVersion = Utilities.GetFileVersion(pathToYoutubeDl);

                var client = new GitHubClient(new ProductHeaderValue("youtube-dl"));
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

            Model.IsIndeterminate = false;
        }

        private void DownloadButtonCommand()
        {
            if (string.IsNullOrWhiteSpace(DownloadLink))
            {
                Model.StandardOutput = "Empty link";
                return;
            }
           
            Model.DownloadButtonClick(DownloadLink, SelectedQuality);
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
