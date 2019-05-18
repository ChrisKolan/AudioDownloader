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
            catch (Exception exception)
            {
                model.StandardOutput = "Status: idle. Failed to update Audio Downloader.\n" + exception.ToString();
                model.EnableInteractions();
                return;
            }

            model.StandardOutput = "Status: idle.";
            model.EnableInteractions();
        }
    }
}
