using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Unit
{
    [TestClass]
    public class UrlHelperTest
    {
        [TestMethod]
        public void TestIsExternalLink()
        {
            Assert.IsTrue(UrlHelper.IsExternalLink("http://site.com"));
            Assert.IsTrue(UrlHelper.IsExternalLink("https://site.com"));
            Assert.IsTrue(UrlHelper.IsExternalLink("//site.com"));
        }

        [TestMethod]
        public void TestNormalizeUrl()
        {
            Assert.AreEqual(UrlHelper.NormalizeUrl("http://site.com"), "http://site.com");
            Assert.AreEqual(UrlHelper.NormalizeUrl("http://site.com/#"), "http://site.com");
            Assert.AreEqual(UrlHelper.NormalizeUrl("http://site.com/#test"), "http://site.com");
            Assert.AreEqual(UrlHelper.NormalizeUrl("//site.com"), "https://site.com");
            Assert.AreEqual(UrlHelper.NormalizeUrl("https://site.com/"), "https://site.com");
        }

        [TestMethod]
        public void TestGetPartialUrl()
        {
            Assert.AreEqual(UrlHelper.GetPartialUrl("http://site.com"), string.Empty);
            Assert.AreEqual(UrlHelper.GetPartialUrl("http://site.com/#"), "/#");
            Assert.AreEqual(UrlHelper.GetPartialUrl("http://site.com/#test"), "/#test");
            Assert.AreEqual(UrlHelper.GetPartialUrl("http://site.com/test#part"), "#part");
        }

        [TestMethod]
        public void TestExtractRoot()
        {
            Assert.AreEqual(UrlHelper.ExtractRoot("http://site.com"), "http://site.com");
            Assert.AreEqual(UrlHelper.ExtractRoot("http://site.com/sub-page"), "http://site.com");
            Assert.AreEqual(UrlHelper.ExtractRoot("http://site.com/sub-page/sub-sub-page"), "http://site.com");
        }

        [TestMethod]
        public void TestBuildRelativeUri()
        {
            Assert.AreEqual(UrlHelper.BuildRelativeUri("http://site.com", "sub-page"), "http://site.com/sub-page");
            Assert.AreEqual(UrlHelper.BuildRelativeUri("http://site.com/", "sub-page"), "http://site.com/sub-page");
            Assert.AreEqual(UrlHelper.BuildRelativeUri("http://site.com/", "/sub-page"), "http://site.com/sub-page");
        }
    }
}