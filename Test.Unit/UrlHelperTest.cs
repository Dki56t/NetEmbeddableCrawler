using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Unit
{
    
    public class UrlHelperTest
    {
        [Fact]
        public void TestIsExternalLink()
        {
            Assert.IsTrue(UrlHelper.IsExternalLink("http://site.com"));
            Assert.IsTrue(UrlHelper.IsExternalLink("https://site.com"));
            Assert.IsTrue(UrlHelper.IsExternalLink("//site.com"));
        }

        [Fact]
        public void TestNormalizeUrl()
        {
            Assert.AreEqual(UrlHelper.NormalizeUrl("http://site.com"), "http://site.com");
            Assert.AreEqual(UrlHelper.NormalizeUrl("http://site.com/#"), "http://site.com");
            Assert.AreEqual(UrlHelper.NormalizeUrl("http://site.com/#test"), "http://site.com");
            Assert.AreEqual(UrlHelper.NormalizeUrl("//site.com"), "https://site.com");
            Assert.AreEqual(UrlHelper.NormalizeUrl("https://site.com/"), "https://site.com");
        }

        [Fact]
        public void TestGetPartialUrl()
        {
            Assert.AreEqual(UrlHelper.GetPartialUrl("http://site.com"), string.Empty);
            Assert.AreEqual(UrlHelper.GetPartialUrl("http://site.com/#"), "/#");
            Assert.AreEqual(UrlHelper.GetPartialUrl("http://site.com/#test"), "/#test");
            Assert.AreEqual(UrlHelper.GetPartialUrl("http://site.com/test#part"), "#part");
        }

        [Fact]
        public void TestExtractRoot()
        {
            Assert.AreEqual(UrlHelper.ExtractRoot("http://site.com"), "http://site.com");
            Assert.AreEqual(UrlHelper.ExtractRoot("http://site.com/sub-page"), "http://site.com");
            Assert.AreEqual(UrlHelper.ExtractRoot("http://site.com/sub-page/sub-sub-page"), "http://site.com");
        }

        [Fact]
        public void TestBuildRelativeUri()
        {
            Assert.AreEqual(UrlHelper.BuildRelativeUri("http://site.com", "sub-page"), "http://site.com/sub-page");
            Assert.AreEqual(UrlHelper.BuildRelativeUri("http://site.com/", "sub-page"), "http://site.com/sub-page");
            Assert.AreEqual(UrlHelper.BuildRelativeUri("http://site.com/", "/sub-page"), "http://site.com/sub-page");
        }
    }
}