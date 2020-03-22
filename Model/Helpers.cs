using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
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

        public static bool Pinger(out PingException pingException)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            var hostNameOrAddress = "youtube.com";

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 250;
            PingReply reply;

            try
            {
                reply = pingSender.Send(hostNameOrAddress, timeout, buffer, options);
            }
            catch (PingException exception)
            {
                pingException = exception;
                return false;
            }
            if (reply.Status != IPStatus.Success)
            {
                //Console.WriteLine("Address: {0}", reply.Address.ToString());
                //Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
                //Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                //Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
                //Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
                pingException = new PingException("Sending of an Internet Control Message Protocol not successful");
                return false;
            }
            pingException = new PingException("Sending of an Internet Control Message Protocol successful");
            return true;
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
            if (selectedQuatiy != null)
            {
                return metadata + "--restrict-filenames -o audio\\video\\" + date + title + escapedDownloadLink;
            }
            if (quality == null)
            {
                return metadata + "--restrict-filenames -o audio\\" + date + title + escapedDownloadLink;
            }
            return metadata + "--audio-quality " + quality + " --restrict-filenames -o audio\\" + date + "Q" + quality + title + escapedDownloadLink;
        }
    }
}
