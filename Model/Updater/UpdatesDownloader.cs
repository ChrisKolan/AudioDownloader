using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class UpdatesDownloader
    {
        public static async Task DownloadUpdatesAsync(Model model)
        {
            var updatesCheck = await Task.Run(() => UpdatesNeeded.CheckAsync());
            var pathToExe = Assembly.GetEntryAssembly().Location;
            var pathToExeFolder = System.IO.Path.GetDirectoryName(pathToExe);
            var client = new GitHubClient(new ProductHeaderValue("audio-downloader"));
            client.SetRequestTimeout(TimeSpan.FromSeconds(20));
            var releasesYoutubeDl = await client.Repository.Release.GetLatest("ytdl-org", "youtube-dl");
            var releasesAudioDl= await client.Repository.Release.GetLatest("ChrisKolan", "audio-downloader");

            if (updatesCheck["audio-downloader"] == true)
            {
                model.StandardOutput = "Downloading new Audio Downloader version";
                var pathToAudioDownloaderTempFolder = pathToExeFolder + @"\AudioDownloader.zip";
                var latestAsset = await client.Repository.Release.GetAllAssets("ChrisKolan", "audio-downloader", releasesAudioDl.Id);
                var latestUri = latestAsset[0].BrowserDownloadUrl;
                var response = await client.Connection.Get<object>(new Uri(latestUri), new Dictionary<string, string>(), "application/octet-stream");
                var responseData = response.HttpResponse.Body;
                System.IO.File.WriteAllBytes(pathToAudioDownloaderTempFolder, (byte[])responseData);

                RenameFilesInFolder.Rename();
                Deleter.DeleteBinFolderContents();
                Unzipper.Unzip();
                ApplicationRestarter.Restart();

                model.StandardOutput = "Ready. Updated Audio Downloader to latest version.";
            }
            else if  (updatesCheck["youtube-dl"] == true)
            {
                model.StandardOutput = "Downloading new Youtube-dl version";
                var pathToYoutubeDl = pathToExeFolder + @"\bin\youtube-dl.exe";
                var latestAsset = await client.Repository.Release.GetAllAssets("ytdl-org", "youtube-dl", releasesYoutubeDl.Id);
                var latestUri = latestAsset[7].BrowserDownloadUrl;
                var response = await client.Connection.Get<object>(new Uri(latestUri), new Dictionary<string, string>(), "application/octet-stream");
                var responseData = response.HttpResponse.Body;
                System.IO.File.WriteAllBytes(pathToYoutubeDl, (byte[])responseData);
                model.StandardOutput = "Ready. Updated Youtube-dl to latest version.";
            }
        }
    }
}
