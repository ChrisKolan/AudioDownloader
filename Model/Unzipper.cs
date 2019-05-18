using System.IO.Compression;
using System.Reflection;

namespace Model
{
    class Unzipper
    {
        public static void Unzip()
        {
            var pathToExe = Assembly.GetEntryAssembly().Location;
            var pathToExeFolder = System.IO.Path.GetDirectoryName(pathToExe);
            var pathToYoutubeDl = pathToExeFolder + @"\unzip";
            var pathToAudioDownloaderZipped = pathToExeFolder + @"\AudioDownloader.zip";

            ZipFile.ExtractToDirectory(pathToAudioDownloaderZipped, pathToExeFolder);
        }
    }
}