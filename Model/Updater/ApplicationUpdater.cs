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
            model.StandardOutput = "Status: checking for updates.";
            model.DisableInteractions();

            try
            {
                await UpdatesDownloader.DownloadUpdatesAsync(model);
            }
            catch (Exception)
            {
                model.StandardOutput = "Status: idle. Failed to update Audio Downloader.";
                model.EnableInteractions();
                return;
            }

            model.StandardOutput = "Status: idle.";
            model.EnableInteractions();
        }
    }
}
