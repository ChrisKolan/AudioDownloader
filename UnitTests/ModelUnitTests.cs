using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ModelUnitTests : BaseTest
    {
        Model.Model _model;

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
            var qualities = new List<string> { "raw webm", "raw opus", "raw aac", "raw vorbis", "superb", "best", "better", "optimal", "very good", 
                                               "transparent", "good", "acceptable", "audio book", "worse", "worst" };
            _model.DownloadLink = "https://www.youtube.com/watch?v=4KcQ90UbRsg";
            foreach (var quality in qualities)
            {
                _model.SelectedQuality = quality;
                _model.DownloadButtonClick();
                Thread.Sleep(1000);
                while (!_model.IsComboBoxEnabled)
                {
                    Thread.Sleep(100);
                }
            }
        }
        [TestMethod]
        public void DownloadRawFormats()
        {
            var qualities = new List<string> { "251\twebm", "140\tm4a", "250\twebm", "249\twebm" };
            _model.DownloadLink = "https://www.youtube.com/watch?v=4KcQ90UbRsg";
            foreach (var quality in qualities)
            {
                _model.SelectedQuality = quality;
                _model.DownloadButtonClick();
                Thread.Sleep(1000);
                while (!_model.IsComboBoxEnabled)
                {
                    Thread.Sleep(100);
                }
            }
        }
        [TestMethod]
        public void DownloadPlayListOne()
        {
            _model.DownloadLink = "https://www.youtube.com/playlist?list=PL9tWYRlGyp4GgQu1liXcY9NT1Geg3Nsok";
            _model.SelectedQuality = "raw aac";
            _model.DownloadButtonClick();
            Thread.Sleep(1000);
            while (!_model.IsComboBoxEnabled)
            {
                Thread.Sleep(100);
            }
        }
        [TestMethod]
        public void DownloadPlayListTwo()
        {
            _model.DownloadLink = "https://www.youtube.com/watch?v=Nxs_mpWt2BA&list=PLczZk1L30r_s_9woWc1ZvhUNA2n_wjICI&index=1";
            _model.SelectedQuality = "transparent";
            _model.DownloadButtonClick();
            Thread.Sleep(1000);
            while (!_model.IsComboBoxEnabled)
            {
                Thread.Sleep(100);
            }
        }
    }
}
