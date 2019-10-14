using Crawler.Logic;
using Xunit;

namespace Tests.UnitTests
{
    public class UrlHelperTests
    {
        [Fact]
        public void ShouldBuildRelativeUri()
        {
            Assert.Equal("http://site.com/sub-page", UrlHelper.BuildRelativeUri("http://site.com", "sub-page"));
            Assert.Equal("http://site.com/sub-page", UrlHelper.BuildRelativeUri("http://site.com/", "sub-page"));
            Assert.Equal("http://site.com/sub-page", UrlHelper.BuildRelativeUri("http://site.com/", "/sub-page"));
        }

        [Fact]
        public void ShouldDetermineIfItIsExternalLink()
        {
            Assert.True(UrlHelper.IsExternalLink("http://site.com"));
            Assert.True(UrlHelper.IsExternalLink("https://site.com"));
            Assert.True(UrlHelper.IsExternalLink("//site.com"));
        }

        [Fact]
        public void ShouldExtractRoot()
        {
            Assert.Equal("http://site.com", UrlHelper.ExtractRoot("http://site.com"));
            Assert.Equal("http://site.com", UrlHelper.ExtractRoot("http://site.com/sub-page"));
            Assert.Equal("http://site.com", UrlHelper.ExtractRoot("http://site.com/sub-page/sub-sub-page"));
        }

        [Fact]
        public void ShouldGetPartialUrl()
        {
            Assert.Equal(UrlHelper.GetPartialUrl("http://site.com"), string.Empty);
            Assert.Equal("/#", UrlHelper.GetPartialUrl("http://site.com/#"));
            Assert.Equal("/#test", UrlHelper.GetPartialUrl("http://site.com/#test"));
            Assert.Equal("#part", UrlHelper.GetPartialUrl("http://site.com/test#part"));
        }

        [Fact]
        public void ShouldNormalizeUrl()
        {
            Assert.Equal("http://site.com", UrlHelper.NormalizeUrl("http://site.com"));
            Assert.Equal("http://site.com", UrlHelper.NormalizeUrl("http://site.com/#"));
            Assert.Equal("http://site.com", UrlHelper.NormalizeUrl("http://site.com/#test"));
            Assert.Equal("https://site.com", UrlHelper.NormalizeUrl("//site.com"));
            Assert.Equal("https://site.com", UrlHelper.NormalizeUrl("https://site.com/"));
            Assert.Null(UrlHelper.NormalizeUrl("//"));
            Assert.Null(UrlHelper.NormalizeUrl(";"));
        }

        [Fact]
        public void ShouldDetermineEqualHosts()
        {
            Assert.True(UrlHelper.EqualHosts("http://site.com", "http://site.com"));
            Assert.True(UrlHelper.EqualHosts("http://SITE.com", "http://site.com"));
            Assert.True(UrlHelper.EqualHosts("https://site.com", "http://site.com"));
            Assert.True(UrlHelper.EqualHosts("https://site.com", "http://site.com/page"));
            Assert.False(UrlHelper.EqualHosts("https://site1.com", "http://site.com"));
            Assert.False(UrlHelper.EqualHosts("https://site.com", "http://site.net"));
        }
    }
}