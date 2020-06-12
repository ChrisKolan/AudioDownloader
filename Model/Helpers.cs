using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

namespace Model
{
    public static class HelpersModel
    {
        public static string ToolTipFolder 
        {
            get { return Localization.Properties.Resources.ToolTipFolder + ApplicationPaths.GetAudioPath(); }
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
                finishedMessage = Localization.Properties.Resources.FinishedMessageDownloadFinishedMp3;
            }
            else if (selectedQuality.Contains("flac"))
            {
                command = youTubeBeginCommand + "--extract-audio --audio-format flac" + GenerateConstantCommand(date, escapedDownloadLink, null, null);
                finishedMessage = Localization.Properties.Resources.FinishedMessageDownloadFinishedFlac;
            }
            else if (selectedQuality.Contains("raw webm"))
            {
                command = youTubeBeginCommand + "--format bestaudio[ext=webm]" + GenerateConstantCommand(date, escapedDownloadLink, null, null);
                finishedMessage = Localization.Properties.Resources.FinishedMessageDownloadFinished;
            }
            else if (selectedQuality.Contains("raw opus"))
            {
                command = youTubeBeginCommand + "--format bestaudio[acodec=opus] --extract-audio" + GenerateConstantCommand(date, escapedDownloadLink, null, null);
                finishedMessage = Localization.Properties.Resources.FinishedMessageDownloadFinished;
            }
            else if (selectedQuality.Contains("raw aac"))
            {
                command = youTubeBeginCommand + "--format bestaudio[ext=m4a]" + GenerateConstantCommand(date, escapedDownloadLink, null, null);
                finishedMessage = Localization.Properties.Resources.FinishedMessageDownloadFinished;
            }
            else if (selectedQuality.Split(' ').First().All(char.IsDigit))
            {
                var formatCode = selectedQuality.Split(' ').First();
                var format = selectedQuality.Split(' ').Last();
                command = youTubeBeginCommand + "--format " + formatCode + " --extract-audio  --audio-format " + format + GenerateConstantCommand(date, escapedDownloadLink, null, null);
                finishedMessage = Localization.Properties.Resources.FinishedMessageDownloadFinished;
            }
            else if (selectedQuality.Contains("video"))
            {
                command = youTubeBeginCommand + GenerateConstantCommand(date, escapedDownloadLink, null, selectedQuality);
                finishedMessage = Localization.Properties.Resources.FinishedMessageDownloadFinished;
            }
            else
            {
                command = youTubeBeginCommand + "--format bestaudio[acodec=vorbis] --extract-audio" + GenerateConstantCommand(date, escapedDownloadLink, null, null);
                finishedMessage = Localization.Properties.Resources.FinishedMessageDownloadFinished;
            }

            return (command, finishedMessage);
        }

        public static ObservableCollection<string> QualityObservableCollection()
        {
            var quality = new ObservableCollection<string>
            {
                Localization.Properties.Resources.ComboBoxInitial
            };
            return quality;
        }
            
        public static string GetQuality(string selectedQuality)
        {
            Contract.Requires(selectedQuality != null);
            if (selectedQuality.Contains("\t") && selectedQuality.Split('\t').First().All(char.IsDigit))
            {
                var format = FindFormat(selectedQuality);
                var formatCode = selectedQuality.Split('\t').First();
                return formatCode + " " + format;
            }
            else if (selectedQuality.Contains("webm"))
                return "raw webm";
            else if (selectedQuality.Contains("opus"))
                return "raw opus";
            else if (selectedQuality.Contains("aac"))
                return "raw aac";
            else if (selectedQuality.Contains("vorbis"))
                return "raw vorbis";
            else if (selectedQuality.Contains("FLAC"))
                return "flac";
            else if (selectedQuality.Contains("245"))
                return "mp3 0";
            else if (selectedQuality.Contains("225"))
                return "mp3 1";
            else if (selectedQuality.Contains("190"))
                return "mp3 2";
            else if (selectedQuality.Contains("175"))
                return "mp3 3";
            else if (selectedQuality.Contains("165"))
                return "mp3 4";
            else if (selectedQuality.Contains("130"))
                return "mp3 5";
            else if (selectedQuality.Contains("115"))
                return "mp3 6";
            else if (selectedQuality.Contains("100"))
                return "mp3 7";
            else if (selectedQuality.Contains("085"))
                return "mp3 8";
            else if (selectedQuality.Contains("065"))
                return "mp3 9";
            else if (selectedQuality.Contains("YouTube"))
                return "video";
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
                Localization.Properties.Resources.AudioQualitySuperb,
                Localization.Properties.Resources.AudioQualityBest,
                Localization.Properties.Resources.AudioQualityBetter,
                Localization.Properties.Resources.AudioQualityOptimal,
                Localization.Properties.Resources.AudioQualityVeryGood,
                Localization.Properties.Resources.AudioQualityTransparent,
                Localization.Properties.Resources.AudioQualityGood,
                Localization.Properties.Resources.AudioQualityAcceptable,
                Localization.Properties.Resources.AudioQualityAudioBook,
                Localization.Properties.Resources.AudioQualityWorse,
                Localization.Properties.Resources.AudioQualityWorst,
            };
            return qualityDefault;
        }

        public static string StandardOutputLocalizator(string standardOutput)
        {
            if (standardOutput == null)
            {
                return string.Empty;
            }
            standardOutput = standardOutput.Replace("download", Localization.Properties.Resources.ThreadPoolWorkerDownload);
            standardOutput = standardOutput.Replace("ETA", Localization.Properties.Resources.ThreadPoolWorkerEta);
            standardOutput = standardOutput.Replace("of ", Localization.Properties.Resources.ThreadPoolWorkerOf);
            standardOutput = standardOutput.Replace(" at", Localization.Properties.Resources.ThreadPoolWorkerAt);
            return standardOutput.Replace("Downloading webpage", Localization.Properties.Resources.ThreadPoolWorkerDownloadingWebpage);
        }

        public static double GetTaskBarProgressValue(int maximum, int progress)
        {
            return (double)progress / (double)maximum;
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
