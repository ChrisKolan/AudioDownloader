using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public static class UpdatesNeeded
    {
        public static async Task<Dictionary<string, bool>> CheckAsync()
        {
            var updatesChecked = new Dictionary<string, bool>();

            var entitiesToCheck = new List<string>
            {
                { "audio-downloader"},
                { "youtube-dl"}
            };

            var remoteVersions = await Task.Run(() => GitHubVersionProvider.VersionsAsync()).ConfigureAwait(false);
            var localVersions = LocalVersionProvider.Versions();

            for (int i = 0; i < localVersions.Count; i++)
            {
                if (remoteVersions[i] != localVersions[i])
                    updatesChecked[entitiesToCheck[i]] = true;
                else
                    updatesChecked[entitiesToCheck[i]] = false;
            }
            
            return updatesChecked;
        }
    }
}
