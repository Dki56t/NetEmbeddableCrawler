using Crawler.Logic;
using Xunit;

namespace Tests.UnitTests
{
    public class UrlHelperTest
    {
        [Fact]
        public void TestBuildRelativeUri()
        {
            Assert.Equal(UrlHelper.BuildRelativeUri("http://site.com", "sub-page"), "http://site.com/sub-page");
            Assert.Equal(UrlHelper.BuildRelativeUri("http://site.com/", "sub-page"), "http://site.com/sub-page");
            Assert.Equal(UrlHelper.BuildRelativeUri("http://site.com/", "/sub-page"), "http://site.com/sub-page");
        }

        [Fact]
        public void TestExtractRoot()
        {
            Assert.Equal(UrlHelper.ExtractRoot("http://site.com"), "http://site.com");
            Assert.Equal(UrlHelper.ExtractRoot("http://site.com/sub-page"), "http://site.com");
            Assert.Equal(UrlHelper.ExtractRoot("http://site.com/sub-page/sub-sub-page"), "http://site.com");
        }

        [Fact]
        public void TestGetPartialUrl()
        {
            Assert.Equal(UrlHelper.GetPartialUrl("http://site.com"), string.Empty);
            Assert.Equal(UrlHelper.GetPartialUrl("http://site.com/#"), "/#");
            Assert.Equal(UrlHelper.GetPartialUrl("http://site.com/#test"), "/#test");
            Assert.Equal(UrlHelper.GetPartialUrl("http://site.com/test#part"), "#part");
        }

        [Fact]
        public void TestIsExternalLink()
        {
            Assert.True(UrlHelper.IsExternalLink("http://site.com"));
            Assert.True(UrlHelper.IsExternalLink("https://site.com"));
            Assert.True(UrlHelper.IsExternalLink("//site.com"));
        }

        [Fact]
        public void TestNormalizeUrl()
        {
            Assert.Equal(UrlHelper.NormalizeUrl("http://site.com"), "http://site.com");
            Assert.Equal(UrlHelper.NormalizeUrl("http://site.com/#"), "http://site.com");
            Assert.Equal(UrlHelper.NormalizeUrl("http://site.com/#test"), "http://site.com");
            Assert.Equal(UrlHelper.NormalizeUrl("//site.com"), "https://site.com");
            Assert.Equal(UrlHelper.NormalizeUrl("https://site.com/"), "https://site.com");
        }
    }
}