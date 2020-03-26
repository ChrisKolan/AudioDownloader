using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public static class UpdatesDownloader
    {
        public static async Task DownloadUpdatesAsync(ModelClass model)
        {
            Contract.Requires(model != null);
            model.InformationAndExceptionOutput = "Checking for updates";
            var updatesCheck = await Task.Run(() => UpdatesNeeded.CheckAsync()).ConfigureAwait(false);
            var pathToExe = Assembly.GetEntryAssembly().Location;
            var pathToExeFolder = System.IO.Path.GetDirectoryName(pathToExe);
            var client = new GitHubClient(new ProductHeaderValue("audio-downloader"));
            client.SetRequestTimeout(TimeSpan.FromSeconds(20));
            var releasesYoutubeDl = await client.Repository.Release.GetLatest("ytdl-org", "youtube-dl").ConfigureAwait(false);
            var releasesAudioDl= await client.Repository.Release.GetLatest("ChrisKolan", "audio-downloader").ConfigureAwait(false);

            if (updatesCheck["audio-downloader"] == true)
            {
                model.StandardOutput = "Downloading new Audio Downloader version";
                model.InformationAndExceptionOutput = "Downloading new Audio Downloader version";
                var pathToAudioDownloaderTempFolder = pathToExeFolder + @"\AudioDownloader.zip";
                var latestAsset = await client.Repository.Release.GetAllAssets("ChrisKolan", "audio-downloader", releasesAudioDl.Id).ConfigureAwait(false);
                var latestUri = latestAsset[0].BrowserDownloadUrl;
                var response = await client.Connection.Get<object>(new Uri(latestUri), new Dictionary<string, string>(), "application/octet-stream").ConfigureAwait(false);
                var responseData = response.HttpResponse.Body;
                System.IO.File.WriteAllBytes(pathToAudioDownloaderTempFolder, (byte[])responseData);
                model.InformationAndExceptionOutput = "Download finished";

                RenameFilesInFolder.Rename();
                model.InformationAndExceptionOutput = "Renamed files";
                Deleter.DeleteBinFolderContents();
                model.InformationAndExceptionOutput = "Deleted files";
                Unzipper.Unzip();
                model.InformationAndExceptionOutput = "Unzipped files";
                ApplicationRestarter.Restart();
                model.InformationAndExceptionOutput = "Restarting application";
            }
            else if  (updatesCheck["youtube-dl"] == true)
            {
                model.StandardOutput = "Downloading new Youtube-dl version";
                model.InformationAndExceptionOutput  = "Downloading new Youtube-dl version";
                var pathToYoutubeDl = pathToExeFolder + @"\bin\youtube-dl.exe";
                var latestAsset = await client.Repository.Release.GetAllAssets("ytdl-org", "youtube-dl", releasesYoutubeDl.Id).ConfigureAwait(false);
                var latestUri = latestAsset[7].BrowserDownloadUrl;
                var response = await client.Connection.Get<object>(new Uri(latestUri), new Dictionary<string, string>(), "application/octet-stream").ConfigureAwait(false);
                var responseData = response.HttpResponse.Body;
                System.IO.File.WriteAllBytes(pathToYoutubeDl, (byte[])responseData);
                model.StandardOutput = "Ready. Updated Youtube-dl to latest version.";
                model.InformationAndExceptionOutput = "Updated Youtube-dl to latest version";
            }
        }
    }
}
