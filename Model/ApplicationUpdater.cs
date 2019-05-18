using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    class ApplicationUpdater
    {
        public static async Task UpdateAsync(Model model)
        {
            model.StandardOutput = "Status: updating Audio Downloader.";
            model.DisableInteractions();

            try
            {
                await UpdatesDownloader.DownloadUpdatesAsync(model);
                RenameFilesInFolder.Rename();
                Deleter.DeleteBinFolderContents();
                Unzipper.Unzip();
                ApplicationRestarter.Restart();
            }
            catch (Exception)
            {
                model.StandardOutput = "Status: idle. Failed to update Audio Downloader.";
                model.EnableInteractions();
                return;
            }

            model.StandardOutput = "Status: idle. Application updated. Please restart the application.";
            model.EnableInteractions();
        }
    }
}
