using Octokit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public static class GitHubVersionProvider
    {
        public static async Task<List<string>> VersionsAsync()
        {
            var latestReleasesVersions = new List<string>();

            var repositoriesToCheck = new Dictionary<string, string>
            {
                { "ChrisKolan", "audio-downloader" },
                { "ytdl-org", "youtube-dl"}
            };

            var client = new GitHubClient(new ProductHeaderValue("audio-downloader"));
            client.SetRequestTimeout(TimeSpan.FromSeconds(20));

            foreach (var repository in repositoriesToCheck)
            {
                var releases = await client.Repository.Release.GetLatest(repository.Key, repository.Value);
                latestReleasesVersions.Add(releases.TagName);
            }

            return latestReleasesVersions;
        }
    }
}
