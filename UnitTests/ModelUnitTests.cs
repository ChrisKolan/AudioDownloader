using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ModelUnitTests : BaseTest
    {
        private Model.Model _model;
        private static readonly string _pathDll = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string _audioPath = _pathDll + @"\audio\";

        public ModelUnitTests()
        {
            _model = new Model.Model();
            // Give some time to update 
            Thread.Sleep(30000);
        }
        [TestMethod]
        public void EmptyLink()
        {
            _model.DownloadLink = "";
            _model.DownloadButtonClick();
            Assert.IsTrue(_model.StandardOutput == "Empty link");
        }
        [TestMethod]
        public void WhiteSpaceLink()
        {
            _model.DownloadLink = " ";
            _model.DownloadButtonClick();
            Assert.IsTrue(_model.StandardOutput == "Empty link");
        }
        [TestMethod]
        public void YouTubeLinkNotValid()
        {
            _model.DownloadLink = "Not valid link";
            _model.DownloadButtonClick();

            // Give some time to download AudioDownloader.zip
            Thread.Sleep(4000);
            Assert.IsTrue(_model.StandardOutput == "Error. No file downloaded. Updates are needed.");
        }
        [TestMethod]
        public void DownloadInDifferentQuality()
        {
            var qualities = new List<string> { "raw webm", "raw opus", "raw aac", "superb", "best", "better", "optimal", "very good", 
                                               "transparent", "good", "acceptable", "audio book", "worse", "worst" };
            var expectedFileSizes = new List<long> { 3136265, 3089436, 3083006, 39763743, 6472224, 5511768, 4601376, 4180440, 3693096, 3182544, 2800368, 2487984, 2381328, 1924416 };
            _model.DownloadLink = "https://www.youtube.com/watch?v=4KcQ90UbRsg";
            for (int i = 0; i < qualities.Count; i++)
            {
                _model.SelectedQuality = qualities[i];
                _model.DownloadButtonClick();
                Thread.Sleep(1000);
                while (!_model.IsComboBoxEnabled)
                {
                    Thread.Sleep(100);
                }
                var numberOfFiles = NumberOfFilesInDirectory(_audioPath);
                Assert.IsTrue(numberOfFiles == 1);
                var fileName = FileNamesAndPath(_audioPath);
                var actualFileSize = FileSize(fileName[0]);
                var expetedFileSize = expectedFileSizes[i];
                var range = 200;
                Console.WriteLine($"Actual file size: {actualFileSize}, expected file size: {expetedFileSize}. File name: {fileName[0]}.");
                Assert.IsTrue((expetedFileSize - range) < actualFileSize && actualFileSize < (expetedFileSize + range), $"File size outside expected range. Actual file size: {actualFileSize}, expected file size: {expetedFileSize}, +/-range: {range}");
                DeleteFiles(fileName);
            }
        }
        [TestMethod]
        public void DownloadRawFormats()
        {
            var qualities = new List<string> { "251\twebm", "140\tm4a", "250\twebm", "249\twebm" };
            var expectedFileSizes = new List<long> { 3670761, 3083046, 3562178, 3459471 };
            _model.DownloadLink = "https://www.youtube.com/watch?v=4KcQ90UbRsg";
            for (int i = 0; i < qualities.Count; i++)
            {
                _model.SelectedQuality = qualities[i];
                _model.DownloadButtonClick();
                Thread.Sleep(1000);
                while (!_model.IsComboBoxEnabled)
                {
                    Thread.Sleep(100);
                }
                var numberOfFiles = NumberOfFilesInDirectory(_audioPath);
                Assert.IsTrue(numberOfFiles == 1);
                var fileName = FileNamesAndPath(_audioPath);
                var actualFileSize = FileSize(fileName[0]);
                var expetedFileSize = expectedFileSizes[i];
                var range = 200;
                Console.WriteLine($"Actual file size: {actualFileSize}, expected file size: {expetedFileSize}. File name: {fileName[0]}.");
                Assert.IsTrue((expetedFileSize - range) < actualFileSize && actualFileSize < (expetedFileSize + range), $"File size outside expected range. Actual file size: {actualFileSize}, expected file size: {expetedFileSize}, +/-range: {range}");
                DeleteFiles(fileName);
            }
        }
        [TestMethod]
        public void DownloadPlayListOne()
        {
            _model.DownloadLink = "https://www.youtube.com/playlist?list=PL9tWYRlGyp4GgQu1liXcY9NT1Geg3Nsok";
            _model.SelectedQuality = "raw aac";
            var expectedFileSizes = new List<long> { 5083580, 4034396, 3402184, 3631409, 2875099, 3873389, 3458765, 5609895, 4151618 };
            _model.DownloadButtonClick();
            Thread.Sleep(1000);
            while (!_model.IsComboBoxEnabled)
            {
                Thread.Sleep(100);
            }
            var numberOfFiles = NumberOfFilesInDirectory(_audioPath);
            Assert.IsTrue(numberOfFiles == 9);
            var fileNames = FileNamesAndPath(_audioPath);
            for (int i = 0; i < numberOfFiles; i++)
            {
                var actualFileSize = FileSize(fileNames[i]);
                var expetedFileSize = expectedFileSizes[i];
                var range = 200;
                Console.WriteLine($"Actual file size: {actualFileSize}, expected file size: {expetedFileSize}. File name: {fileNames[i]}.");
                Assert.IsTrue((expetedFileSize - range) < actualFileSize && actualFileSize < (expetedFileSize + range), $"File size outside expected range. Actual file size: {actualFileSize}, expected file size: {expetedFileSize}, +/-range: {range}");
            }
            DeleteFiles(fileNames);
        }
        [TestMethod]
        public void DownloadPlayListTwo()
        {
            _model.DownloadLink = "https://www.youtube.com/watch?v=Nxs_mpWt2BA&list=PLczZk1L30r_s_9woWc1ZvhUNA2n_wjICI&index=1";
            _model.SelectedQuality = "raw aac";
            var expectedFileSizes = new List<long> { 2915430, 6544910, 4142954, 7978964, 5816029, 3497213, 3527147, 3649456, 4245348, 2883434, 3484768, 2475309, 3455726, 3840186 };
            _model.DownloadButtonClick();
            Thread.Sleep(1000);
            while (!_model.IsComboBoxEnabled)
            {
                Thread.Sleep(100);
            }
            var numberOfFiles = NumberOfFilesInDirectory(_audioPath);
            Assert.IsTrue(numberOfFiles == 14);
            var fileNames = FileNamesAndPath(_audioPath);
            for (int i = 0; i < numberOfFiles; i++)
            {
                var actualFileSize = FileSize(fileNames[i]);
                var expetedFileSize = expectedFileSizes[i];
                var range = 200;
                Console.WriteLine($"Actual file size: {actualFileSize}, expected file size: {expetedFileSize}. File name: {fileNames[i]}.");
                Assert.IsTrue((expetedFileSize - range) < actualFileSize && actualFileSize < (expetedFileSize + range), $"File size outside expected range. Actual file size: {actualFileSize}, expected file size: {expetedFileSize}, +/-range: {range}");
            }
            DeleteFiles(fileNames);
        }
    }
}
