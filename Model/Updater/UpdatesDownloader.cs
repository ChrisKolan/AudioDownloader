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
            model.InformationAndExceptionOutput = Localization.Properties.Resources.StandardOutputCheckingForUpdates;
            model.Log.Information(Localization.Properties.Resources.StandardOutputCheckingForUpdates);
            int configTimeoutSeconds = 40;
            if (int.TryParse(ApplicationSettingsProvider.GetValue("TimeoutInSeconds"), out int timeout))
            {
                configTimeoutSeconds = timeout;
            }
            var updatesCheck = await Task.Run(() => UpdatesNeeded.CheckAsync()).ConfigureAwait(false);
            var pathToExe = Assembly.GetEntryAssembly().Location;
            var pathToExeFolder = System.IO.Path.GetDirectoryName(pathToExe);
            var client = new GitHubClient(new ProductHeaderValue("audio-downloader"));
            client.SetRequestTimeout(TimeSpan.FromSeconds(configTimeoutSeconds));
            var releasesYoutubeDl = await client.Repository.Release.GetLatest("ytdl-org", "youtube-dl").ConfigureAwait(false);
            var releasesAudioDl= await client.Repository.Release.GetLatest("ChrisKolan", "audio-downloader").ConfigureAwait(false);

            if (updatesCheck["audio-downloader"] == true)
            {
                model.StandardOutput = Localization.Properties.Resources.StandardOutputDownloadingNewAudioDownloaderVersion;
                model.InformationAndExceptionOutput = Localization.Properties.Resources.StandardOutputDownloadingNewAudioDownloaderVersion;
                model.Log.Information(Localization.Properties.Resources.StandardOutputDownloadingNewAudioDownloaderVersion);
                var pathToAudioDownloaderTempFolder = pathToExeFolder + @"\AudioDownloader.zip";
                var latestAsset = await client.Repository.Release.GetAllAssets("ChrisKolan", "audio-downloader", releasesAudioDl.Id).ConfigureAwait(false);
                var latestUri = latestAsset[0].BrowserDownloadUrl;
                var response = await client.Connection.Get<object>(new Uri(latestUri), new Dictionary<string, string>(), "application/octet-stream").ConfigureAwait(false);
                var responseData = response.HttpResponse.Body;
                System.IO.File.WriteAllBytes(pathToAudioDownloaderTempFolder, (byte[])responseData);
                model.InformationAndExceptionOutput = Localization.Properties.Resources.DownloadFinished;
                model.Log.Information(Localization.Properties.Resources.DownloadFinished);

                RenameFilesInFolder.Rename();
                model.InformationAndExceptionOutput = Localization.Properties.Resources.RenamedFiles;
                Deleter.DeleteBinFolderContents();
                model.InformationAndExceptionOutput = Localization.Properties.Resources.DeletedFiles;
                Unzipper.Unzip();
                model.InformationAndExceptionOutput = Localization.Properties.Resources.UnzippedFiles;
                ApplicationRestarter.Restart();
                model.InformationAndExceptionOutput = Localization.Properties.Resources.RestartingApplication;
            }
            else if  (updatesCheck["youtube-dl"] == true)
            {
                model.StandardOutput = Localization.Properties.Resources.StandardOutputDownloadingNewYoutubeDlVersion;
                model.InformationAndExceptionOutput  = Localization.Properties.Resources.StandardOutputDownloadingNewYoutubeDlVersion;
                model.Log.Information(Localization.Properties.Resources.StandardOutputDownloadingNewYoutubeDlVersion);
                var pathToYoutubeDl = pathToExeFolder + @"\bin\youtube-dl.exe";
                var latestAsset = await client.Repository.Release.GetAllAssets("ytdl-org", "youtube-dl", releasesYoutubeDl.Id).ConfigureAwait(false);
                var latestUri = latestAsset[7].BrowserDownloadUrl;
                var response = await client.Connection.Get<object>(new Uri(latestUri), new Dictionary<string, string>(), "application/octet-stream").ConfigureAwait(false);
                var responseData = response.HttpResponse.Body;
                System.IO.File.WriteAllBytes(pathToYoutubeDl, (byte[])responseData);
                model.StandardOutput = Localization.Properties.Resources.StandardOutputReadyUpdatedYoutubeDlToLatestVersion;
                model.InformationAndExceptionOutput = Localization.Properties.Resources.StandardOutputReadyUpdatedYoutubeDlToLatestVersion;
                model.Log.Information(Localization.Properties.Resources.StandardOutputReadyUpdatedYoutubeDlToLatestVersion);
            }
        }
    }
}
