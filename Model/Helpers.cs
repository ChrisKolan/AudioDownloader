using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public static class Helpers
    {
        public static (string command, string finishedMessage) CreateCommandAndMessage(string selectedQuality, string date, string downloadLink)
        {
            string command, finishedMessage;
            var youTubeBeginCommand = "/C bin\\youtube-dl.exe ";
            var escapedDownloadLink = "\"" + downloadLink + "\"";

            if (selectedQuality.Contains("mp3"))
            {
                var quality = GetQuality(selectedQuality);
                command = youTubeBeginCommand + "--extract-audio --audio-format mp3" + GenerateConstantCommand(date, escapedDownloadLink, quality);
                finishedMessage = "Download finished. Now transcoding to mp3. This may take a while. Processing.";
            }
            else if (selectedQuality.Contains("flac"))
            {
                command = youTubeBeginCommand + "--extract-audio --audio-format flac" + GenerateConstantCommand(date, escapedDownloadLink, null);
                finishedMessage = "Download finished. Now transcoding to FLAC. This may take a while. Processing.";
            }
            else if (selectedQuality.Contains("raw webm"))
            {
                command = youTubeBeginCommand + "--format bestaudio[ext=webm]" + GenerateConstantCommand(date, escapedDownloadLink, null);
                finishedMessage = "Download finished";
            }
            else if (selectedQuality.Contains("raw opus"))
            {
                command = youTubeBeginCommand + "--format bestaudio[acodec=opus] --extract-audio" + GenerateConstantCommand(date, escapedDownloadLink, null);
                finishedMessage = "Download finished";
            }
            else if (selectedQuality.Contains("raw aac"))
            {
                command = youTubeBeginCommand + "--format bestaudio[ext=m4a]" + GenerateConstantCommand(date, escapedDownloadLink, null);
                finishedMessage = "Download finished";
            }
            else if (selectedQuality.Split(' ').First().All(char.IsDigit))
            {
                var formatCode = selectedQuality.Split(' ').First();
                var format = selectedQuality.Split(' ').Last();
                command = youTubeBeginCommand + "--format " + formatCode + " --extract-audio  --audio-format " + format + GenerateConstantCommand(date, escapedDownloadLink, null);
                finishedMessage = "Download finished";
            }
            else
            {
                command = youTubeBeginCommand + "--format bestaudio[acodec=vorbis] --extract-audio" + GenerateConstantCommand(date, escapedDownloadLink, null);
                finishedMessage = "Download finished";
            }

            return (command, finishedMessage);
        }

        private static string GetQuality(string selectedQuality)
        {
            string[] qualityArray = selectedQuality.Split(' ');

            return qualityArray[1];
        }

        private static string GenerateConstantCommand(string date, string escapedDownloadLink, string quality)
        {
            if (quality == null)
            {
                return " --ignore-errors --no-mtime --add-metadata --restrict-filenames -o audio\\" + date + "-%(playlist_index)s-%(title)s-%(id)s.%(ext)s " + escapedDownloadLink;
            }
            return " --ignore-errors --no-mtime --add-metadata --audio-quality " + quality + " --restrict-filenames -o audio\\" + date + "Q" + quality + "-%(playlist_index)s-%(title)s-%(id)s.%(ext)s " + escapedDownloadLink;
        }
    }
}
