using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052: Static holder types should be Static or NotInheritable")]
    public class BaseTest
    {
        public static int NumberOfFilesInDirectory(string path)
        {
            return System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(path), "*", System.IO.SearchOption.TopDirectoryOnly).Length;
        }

        public static string[] FileNamesAndPath(string path)
        {
            return System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(path), "*").ToArray();
        }

        public static void DeleteFiles(string[] fileNames)
        {
            Contract.Requires(fileNames != null);
            foreach (var fileName in fileNames)
            {
                File.Delete(fileName);
            }
        }

        public static long FileSize(string path)
        {
            return new FileInfo(path).Length;
        }

        public static void Erase(DirectoryInfo directory)
        {
            Contract.Requires(directory != null);
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
            foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }
    }
}
