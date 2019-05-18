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
            //var updatesCheck = await Task.Run(_ => UpdatesDownloader.DownloadUpdatesAsync());
            model.StandardOutput = "Status: updating application.";
            model.DisableInteractions();

            await UpdatesDownloader.DownloadUpdatesAsync(model);
            RenameFilesInFolder.Rename();
            Deleter.DeleteBinFolderContents();
            Unzipper.Unzip();
            Deleter.DeleteOldFiles();

            model.StandardOutput = "Status: idle. Application updated. Please restart the application.";
            model.EnableInteractions();
        }
    }
}
