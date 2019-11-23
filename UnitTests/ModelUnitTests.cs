using System;
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
        }

        [TestMethod]
        public void EmptyLink()
        {
            _model.DownloadLink = "";
            _model.DownloadButtonClick();
            Assert.IsTrue(_model.StandardOutput == "Empty link");
        }

        [TestMethod]
        public void YouTubeLinkNotValid()
        {
            _model.DownloadLink = "Not valid link";
            _model.DownloadButtonClick();

            // Give some time to download AudioDownloader.zip
            Thread.Sleep(30000);
            Assert.IsTrue(_model.StandardOutput == "YouTube link not valid");
        }

        [TestMethod]
        public void DefaultQuality()
        {
            // https://www.youtube.com/playlist?list=PL9tWYRlGyp4GgQu1liXcY9NT1Geg3Nsok
            _model.DownloadLink = "https://www.youtube.com/watch?v=4KcQ90UbRsg";
            //_model.SelectedQuality = _model.Quality[7];
            _model.DownloadButtonClick();
        }
    }
}
