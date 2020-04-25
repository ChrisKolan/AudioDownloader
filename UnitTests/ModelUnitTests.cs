using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ModelUnitTests : BaseTest
    {
        private static readonly string _pathDll = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string _audioPath = _pathDll + @"\audio\";
        private static readonly string _audioAndVideoPath = _pathDll + @"\audio\video\";

        [TestMethod]
        public void A000FolderButtonClick()
        {
            var model = new Model.ModelClass();
            // Give some time to update 
            Thread.Sleep(30000);
            model.FolderButtonClick();
            model.Dispose();
        }
        [TestMethod]
        public void A001EmptyLink()
        {
            var model = new Model.ModelClass();
            model.DownloadLink = "";
            Assert.IsFalse(model.StandardOutput == "Failed to update. Click here to download manually.", "Failed to update. " + model.InformationAndExceptionOutput);
            model.DownloadButtonClick();
            Assert.IsTrue(model.StandardOutput == "Empty link");
            Console.WriteLine(model.InformationAndExceptionOutput);
            model.Dispose();
        }
        [TestMethod]
        public void A002WhiteSpaceLink()
        {
            var model = new Model.ModelClass();
            model.DownloadLink = " ";
            model.DownloadButtonClick();
            Assert.IsTrue(model.StandardOutput == "Empty link");
            model.Dispose();
        }
        [TestMethod]
        public void A003YouTubeLinkNotValid()
        {
            var model = new Model.ModelClass();
            model.DownloadLink = "Not valid link";
            model.DownloadButtonClick();
            Thread.Sleep(4000);
            Assert.IsTrue(model.StandardOutput == "Error. No file downloaded. Updates are needed.");
            model.Dispose();
        }
        [TestMethod]
        public void A004DownloadInDifferentQuality()
        {
            var model = new Model.ModelClass();
            var qualities = new List<string> { "raw webm", "raw opus", "raw aac", "superb", "best", "better", "optimal", "very good",
                                               "transparent", "good", "acceptable", "audio book", "worse", "worst" };
            var expectedFileSizes = new List<long> { 3136081, 3089476, 3083046, 39768753, 6469840, 5518168, 4604680, 4178104, 3692512, 3183856, 2800840, 2486776, 2379040, 1923664 };
            model.DownloadLink = "https://www.youtube.com/watch?v=4KcQ90UbRsg";
            for (int i = 0; i < qualities.Count; i++)
            {
                model.SelectedQuality = qualities[i];
                model.DownloadButtonClick();
                Thread.Sleep(1000);
                while (!model.IsComboBoxEnabled)
                {
                    Thread.Sleep(100);
                }
                var numberOfFiles = NumberOfFilesInDirectory(_audioPath);
                Assert.IsTrue(numberOfFiles == 1, $"Wrong number of files. Expected number of files: 1, actual number of files: {numberOfFiles}");
                var fileName = FileNamesAndPath(_audioPath);
                var actualFileSize = FileSize(fileName[0]);
                var expetedFileSize = expectedFileSizes[i];
                Console.WriteLine($"Actual file size: {actualFileSize}, expected file size: {expetedFileSize}. Difference: {actualFileSize - expetedFileSize}. File name: {fileName[0]}.");
                Assert.IsTrue(actualFileSize == expetedFileSize, $"Not expected file size. Actual file size: {actualFileSize}, expected file size: {expetedFileSize}, difference: {actualFileSize - expetedFileSize}.");
                DeleteFiles(fileName);
            }
            model.Dispose();
        }
        [TestMethod]
        public void A005DownloadRawFormats()
        {
            var model = new Model.ModelClass();
            var qualities = new List<string> { "251\twebm", "140\tm4a", "250\twebm", "249\twebm" };
            var expectedFileSizes = new List<long> { 3663749, 3083046, 3560180, 3454030 };
            model.DownloadLink = "https://www.youtube.com/watch?v=4KcQ90UbRsg";
            for (int i = 0; i < qualities.Count; i++)
            {
                model.SelectedQuality = qualities[i];
                model.DownloadButtonClick();
                Thread.Sleep(1000);
                while (!model.IsComboBoxEnabled)
                {
                    Thread.Sleep(100);
                }
                var numberOfFiles = NumberOfFilesInDirectory(_audioPath);
                Assert.IsTrue(numberOfFiles == 1, $"Wrong number of files. Expected number of files: 1, actual number of files: {numberOfFiles}");
                var fileName = FileNamesAndPath(_audioPath);
                var actualFileSize = FileSize(fileName[0]);
                var expetedFileSize = expectedFileSizes[i];
                Console.WriteLine($"Actual file size: {actualFileSize}, expected file size: {expetedFileSize}. Difference: {actualFileSize - expetedFileSize}. File name: {fileName[0]}.");
                Assert.IsTrue(actualFileSize == expetedFileSize, $"Not expected file size. Actual file size: {actualFileSize}, expected file size: {expetedFileSize}, difference: {actualFileSize - expetedFileSize}.");
                DeleteFiles(fileName);
            }
            model.Dispose();
        }
        [TestMethod]
        public void A006DownloadAudioAndVideo()
        {
            var model = new Model.ModelClass();
            var qualities = new List<string> { "Audio and video" };
            var expectedFileSizes = new List<long> { 70202974 };
            model.DownloadLink = "https://www.youtube.com/watch?v=4KcQ90UbRsg";
            for (int i = 0; i < qualities.Count; i++)
            {
                model.SelectedQuality = qualities[i];
                model.DownloadButtonClick();
                Thread.Sleep(1000);
                while (!model.IsComboBoxEnabled)
                {
                    Thread.Sleep(100);
                }
                var numberOfFiles = NumberOfFilesInDirectory(_audioAndVideoPath);
                Assert.IsTrue(numberOfFiles == 1, $"Wrong number of files. Expected number of files: 1, actual number of files: {numberOfFiles}");
                var fileName = FileNamesAndPath(_audioAndVideoPath);
                var actualFileSize = FileSize(fileName[0]);
                var expetedFileSize = expectedFileSizes[i];
                Console.WriteLine($"Actual file size: {actualFileSize}, expected file size: {expetedFileSize}. Difference: {actualFileSize - expetedFileSize}. File name: {fileName[0]}.");
                Assert.IsTrue(actualFileSize == expetedFileSize, $"Not expected file size. Actual file size: {actualFileSize}, expected file size: {expetedFileSize}, difference: {actualFileSize - expetedFileSize}.");
                DeleteFiles(fileName);
            }
            model.Dispose();
        }
        [TestMethod]
        public void A007DownloadPlayListOne()
        {
            var model = new Model.ModelClass();
            model.DownloadLink = "https://www.youtube.com/playlist?list=PL9tWYRlGyp4GgQu1liXcY9NT1Geg3Nsok";
            model.SelectedQuality = "raw aac";
            var expectedFileSizes = new List<long> { 5083565, 4034396, 3402184, 3631459, 2875099, 3873389, 3458799, 5609895, 4151596 };
            model.DownloadButtonClick();
            Thread.Sleep(1000);
            var allowedSizeDifference = 100;
            while (!model.IsComboBoxEnabled)
            {
                Thread.Sleep(100);
            }
            var numberOfFiles = NumberOfFilesInDirectory(_audioPath);
            Assert.IsTrue(numberOfFiles == expectedFileSizes.Count, $"Wrong number of files. Expected number of files: {expectedFileSizes.Count}, actual number of files: {numberOfFiles}");
            var fileNames = FileNamesAndPath(_audioPath);
            Console.WriteLine($"Allowed size difference: {allowedSizeDifference}.");
            for (int i = 0; i < numberOfFiles; i++)
            {
                var actualFileSize = FileSize(fileNames[i]);
                var expetedFileSize = expectedFileSizes[i];
                Console.WriteLine($"Actual file size: {actualFileSize}, expected file size: {expetedFileSize}. Difference: {actualFileSize - expetedFileSize}. File name: {fileNames[i]}.");
                Assert.IsTrue((expetedFileSize - allowedSizeDifference <= actualFileSize) && (actualFileSize <= expetedFileSize + allowedSizeDifference), $"Not expected file size. Actual file size: {actualFileSize}, expected file size: {expetedFileSize}, difference: {actualFileSize - expetedFileSize}.");
            }
            model.Dispose();
        }
        [TestMethod]
        public void A008DownloadPlayListTwo()
        {
            var model = new Model.ModelClass();
            model.DownloadLink = "https://www.youtube.com/watch?v=Nxs_mpWt2BA&list=PLczZk1L30r_s_9woWc1ZvhUNA2n_wjICI&index=1";
            model.SelectedQuality = "raw aac";
            var expectedFileSizes = new List<long> { 2915430, 6544910, 4142954, 7978914, 5816029, 3563543, 3527081, 3649456, 4245393, 2883786, 3484737, 2475309, 3455726, 3840186 };
            model.DownloadButtonClick();
            Thread.Sleep(1000);
            var allowedSizeDifference = 100;
            while (!model.IsComboBoxEnabled)
            {
                Thread.Sleep(100);
            }
            var numberOfFiles = NumberOfFilesInDirectory(_audioPath);
            Assert.IsTrue(numberOfFiles == expectedFileSizes.Count, $"Wrong number of files. Expected number of files: {expectedFileSizes.Count}, actual number of files: {numberOfFiles}");
            var fileNames = FileNamesAndPath(_audioPath);
            Console.WriteLine($"Allowed size difference: {allowedSizeDifference}.");
            for (int i = 0; i < numberOfFiles; i++)
            {
                var actualFileSize = FileSize(fileNames[i]);
                var expetedFileSize = expectedFileSizes[i];
                Console.WriteLine($"Actual file size: {actualFileSize}, expected file size: {expetedFileSize}. Difference: {actualFileSize - expetedFileSize}. File name: {fileNames[i]}.");
                Assert.IsTrue((expetedFileSize - allowedSizeDifference <= actualFileSize) && (actualFileSize <= expetedFileSize + allowedSizeDifference), $"Not expected file size. Actual file size: {actualFileSize}, expected file size: {expetedFileSize}, difference: {actualFileSize - expetedFileSize}.");
            }
            model.Dispose();
        }
        [TestMethod]
        public void A009DownloadAudioAndVideoOtherSite()
        {
            var model = new Model.ModelClass();
            var qualities = new List<string> { "Audio and video" };
            var expectedFileSizes = new List<long> { 46969734 };
            model.IsWebsitesUnlockerSelected = true;
            model.DownloadLink = "https://helpx.adobe.com/creative-cloud/how-to/creative-cloud-overview.html";
            model.SelectedQuality = qualities[0];
            model.DownloadButtonClick();
            Thread.Sleep(10000);
            var numberOfFiles = NumberOfFilesInDirectory(_audioAndVideoPath);
            Assert.IsTrue(numberOfFiles == 1, $"Wrong number of files. Expected number of files: 1, actual number of files: {numberOfFiles}");
            var fileName = FileNamesAndPath(_audioAndVideoPath);
            var actualFileSize = FileSize(fileName[0]);
            var expetedFileSize = expectedFileSizes[0];
            Console.WriteLine($"Actual file size: {actualFileSize}, expected file size: {expetedFileSize}. Difference: {actualFileSize - expetedFileSize}. File name: {fileName[0]}.");
            Assert.IsTrue(actualFileSize == expetedFileSize, $"Not expected file size. Actual file size: {actualFileSize}, expected file size: {expetedFileSize}, difference: {actualFileSize - expetedFileSize}.");
            Console.WriteLine(model.InformationAndExceptionOutput);
            model.Dispose();
        }
        [TestMethod]
        public void A010DownloadPlayListCancel()
        {
            var model = new Model.ModelClass();
            model.DownloadLink = "https://www.youtube.com/playlist?list=PL9tWYRlGyp4GgQu1liXcY9NT1Geg3Nsok";
            model.SelectedQuality = "raw aac";
            model.DownloadButtonClick();
            Thread.Sleep(1000);
            while (!model.IsComboBoxEnabled)
            {
                // give some time to download a couple of files
                Thread.Sleep(7000);
                // simulating cancel click
                model.DownloadButtonClick();
                Thread.Sleep(10000);
            }
            var numberOfFiles = NumberOfFilesInDirectory(_audioPath);
            Assert.IsTrue(numberOfFiles >= 3 && numberOfFiles <= 7, $"Wrong number of files. Expected number of files between 3 and 7. Actual number of files: {numberOfFiles}");
            model.Dispose();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DeleteFiles(FileNamesAndPath(_audioPath));
        }
    }
}
