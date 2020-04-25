using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public static class HelpersModel
    {
        public static string ToolTipFolder 
        {
            get { return "Left mouse click: opens download folder \nRight mouse click: chooses download folder \nCurrent download folder: " + ApplicationPaths.GetAudioPath(); }
        }
        public static (string command, string finishedMessage) CreateCommandAndMessage(string selectedQuality, string downloadLink)
        {
            Contract.Requires(selectedQuality != null);
            string command, finishedMessage;
            var youTubeBeginCommand = "/C bin\\youtube-dl.exe ";
            var escapedDownloadLink = "\"" + downloadLink + "\"";
            var date = DateTime.Now.ToString("yyMMdd", CultureInfo.InvariantCulture);

            if (selectedQuality.Contains("mp3"))
            {
                var quality = GetQualityInternal(selectedQuality);
                command = youTubeBeginCommand + "--extract-audio --audio-format mp3" + GenerateConstantCommand(date, escapedDownloadLink, quality, null);
                finishedMessage = "Download finished. Now transcoding to mp3. This may take a while. Processing.";
            }
            else if (selectedQuality.Contains("flac"))
            {
                command = youTubeBeginCommand + "--extract-audio --audio-format flac" + GenerateConstantCommand(date, escapedDownloadLink, null, null);
                finishedMessage = "Download finished. Now transcoding to FLAC. This may take a while. Processing.";
            }
            else if (selectedQuality.Contains("raw webm"))
            {
                command = youTubeBeginCommand + "--format bestaudio[ext=webm]" + GenerateConstantCommand(date, escapedDownloadLink, null, null);
                finishedMessage = "Download finished";
            }
            else if (selectedQuality.Contains("raw opus"))
            {
                command = youTubeBeginCommand + "--format bestaudio[acodec=opus] --extract-audio" + GenerateConstantCommand(date, escapedDownloadLink, null, null);
                finishedMessage = "Download finished";
            }
            else if (selectedQuality.Contains("raw aac"))
            {
                command = youTubeBeginCommand + "--format bestaudio[ext=m4a]" + GenerateConstantCommand(date, escapedDownloadLink, null, null);
                finishedMessage = "Download finished";
            }
            else if (selectedQuality.Split(' ').First().All(char.IsDigit))
            {
                var formatCode = selectedQuality.Split(' ').First();
                var format = selectedQuality.Split(' ').Last();
                command = youTubeBeginCommand + "--format " + formatCode + " --extract-audio  --audio-format " + format + GenerateConstantCommand(date, escapedDownloadLink, null, null);
                finishedMessage = "Download finished";
            }
            else if (selectedQuality.Contains("video"))
            {
                command = youTubeBeginCommand + GenerateConstantCommand(date, escapedDownloadLink, null, selectedQuality);
                finishedMessage = "Download finished";
            }
            else
            {
                command = youTubeBeginCommand + "--format bestaudio[acodec=vorbis] --extract-audio" + GenerateConstantCommand(date, escapedDownloadLink, null, null);
                finishedMessage = "Download finished";
            }

            return (command, finishedMessage);
        }

        public static ObservableCollection<string> QualityObservableCollection()
        {
            var quality = new ObservableCollection<string>
            {
                "After pasting YouTube link, you can select the audio quality from this list"
            };
            return quality;
        }
            
        public static string GetQuality(string selectedQuality)
        {
            Contract.Requires(selectedQuality != null);
            if (selectedQuality.Contains("raw webm"))
                return "raw webm";
            else if (selectedQuality.Contains("raw opus"))
                return "raw opus";
            else if (selectedQuality.Contains("raw aac"))
                return "raw aac";
            else if (selectedQuality.Contains("raw vorbis"))
                return "raw vorbis";
            else if (selectedQuality.Contains("superb"))
                return "flac";
            else if (selectedQuality.Contains("best"))
                return "mp3 0";
            else if (selectedQuality.Contains("better"))
                return "mp3 1";
            else if (selectedQuality.Contains("optimal"))
                return "mp3 2";
            else if (selectedQuality.Contains("very good"))
                return "mp3 3";
            else if (selectedQuality.Contains("transparent"))
                return "mp3 4";
            else if (selectedQuality.Contains("good"))
                return "mp3 5";
            else if (selectedQuality.Contains("acceptable"))
                return "mp3 6";
            else if (selectedQuality.Contains("audio book"))
                return "mp3 7";
            else if (selectedQuality.Contains("worse"))
                return "mp3 8";
            else if (selectedQuality.Contains("worst"))
                return "mp3 9";
            else if (selectedQuality.Contains("video"))
                return "video";
            else if (selectedQuality.Split('\t').First().All(char.IsDigit))
            {
                var format = FindFormat(selectedQuality);
                var formatCode = selectedQuality.Split('\t').First();
                return formatCode + " " + format;
            }
            else
                return "mp3 4";
        }

        public static string FindFormat(string selectedQuality)
        {
            Contract.Requires(selectedQuality != null);
            if (selectedQuality.Contains("m4a"))
                return "m4a";
            else
            {
                if (selectedQuality.Contains("opus"))
                    return "opus";
                else
                    return "vorbis";
            }
        }

        public static List<string> QualityDefault()
        {
            var qualityDefault = new List<string>
            {
                "Audio quality: superb \t FLAC lossless compression (Largest flac file size)",
                "Audio quality: best \t Bitrate average: 245 kbit/s, Bitrate range: 220-260 kbit/s, VBR mp3 lossy compression (Large mp3 file size)",
                "Audio quality: better \t Bitrate average: 225 kbit/s, Bitrate range: 190-250 kbit/s, VBR mp3 lossy compression",
                "Audio quality: optimal \t Bitrate average: 190 kbit/s, Bitrate range: 170-210 kbit/s, VBR mp3 lossy compression",
                "Audio quality: very good \t Bitrate average: 175 kbit/s, Bitrate range: 150-195 kbit/s, VBR mp3 lossy compression",
                "Audio quality: transparent \t Bitrate average: 165 kbit/s, Bitrate range: 140-185 kbit/s, VBR mp3 lossy compression (Balanced mp3 file size)",
                "Audio quality: good \t Bitrate average: 130 kbit/s, Bitrate range: 120-150 kbit/s, VBR mp3 lossy compression",
                "Audio quality: acceptable \t Bitrate average: 115 kbit/s, Bitrate range: 100-130 kbit/s, VBR mp3 lossy compression",
                "Audio quality: audio book \t Bitrate average: 100 kbit/s, Bitrate range: 080-120 kbit/s, VBR mp3 lossy compression",
                "Audio quality: worse \t Bitrate average: 085 kbit/s, Bitrate range: 070-105 kbit/s, VBR mp3 lossy compression",
                "Audio quality: worst \t Bitrate average: 065 kbit/s, Bitrate range: 045-085 kbit/s, VBR mp3 lossy compression (Smallest mp3 file size)"
            };
            return qualityDefault;
        }

        private static string GetQualityInternal(string selectedQuality)
        {
            string[] qualityArray = selectedQuality.Split(' ');

            return qualityArray[1];
        }

        private static string GenerateConstantCommand(string date, string escapedDownloadLink, string quality, string selectedQuatiy)
        {
            var metadata = " --ignore-errors --no-mtime --add-metadata "; 
            var title = "-%(playlist_index)s-%(title)s-%(id)s.%(ext)s ";
            var pathToAudioFolder = ApplicationPaths.GetAudioPath();
            var pathToAudioVideoFolder = ApplicationPaths.AudioVideoPath;
            if (selectedQuatiy != null)
            {
                return metadata + "--restrict-filenames -o " + pathToAudioVideoFolder + date + title + escapedDownloadLink;
            }
            if (quality == null)
            {
                return metadata + "--restrict-filenames -o " + pathToAudioFolder + date + title + escapedDownloadLink;
            }
            return metadata + "--audio-quality " + quality + " --restrict-filenames -o " + pathToAudioFolder + date + "Q" + quality + title + escapedDownloadLink;
        }
    }
}
