using Crawler.Logic;
using Xunit;

namespace Tests.UnitTests
{
    public class UrlHelperTest
    {
        [Fact]
        public void TestBuildRelativeUri()
        {
            Assert.Equal("http://site.com/sub-page", UrlHelper.BuildRelativeUri("http://site.com", "sub-page"));
            Assert.Equal("http://site.com/sub-page", UrlHelper.BuildRelativeUri("http://site.com/", "sub-page"));
            Assert.Equal("http://site.com/sub-page", UrlHelper.BuildRelativeUri("http://site.com/", "/sub-page"));
        }

        [Fact]
        public void TestExtractRoot()
        {
            Assert.Equal("http://site.com", UrlHelper.ExtractRoot("http://site.com"));
            Assert.Equal("http://site.com", UrlHelper.ExtractRoot("http://site.com/sub-page"));
            Assert.Equal("http://site.com", UrlHelper.ExtractRoot("http://site.com/sub-page/sub-sub-page"));
        }

        [Fact]
        public void TestGetPartialUrl()
        {
            Assert.Equal(UrlHelper.GetPartialUrl("http://site.com"), string.Empty);
            Assert.Equal("/#", UrlHelper.GetPartialUrl("http://site.com/#"));
            Assert.Equal("/#test", UrlHelper.GetPartialUrl("http://site.com/#test"));
            Assert.Equal("#part", UrlHelper.GetPartialUrl("http://site.com/test#part"));
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
            Assert.Equal("http://site.com", UrlHelper.NormalizeUrl("http://site.com"));
            Assert.Equal("http://site.com", UrlHelper.NormalizeUrl("http://site.com/#"));
            Assert.Equal("http://site.com", UrlHelper.NormalizeUrl("http://site.com/#test"));
            Assert.Equal("https://site.com", UrlHelper.NormalizeUrl("//site.com"));
            Assert.Equal("https://site.com", UrlHelper.NormalizeUrl("https://site.com/"));
        }
    }
}