using Crawler.Logic;
using Shouldly;
using Xunit;

namespace Tests.UnitTests
{
    public class UrlHelperTests
    {
        [Fact]
        public void ShouldBuildRelativeUri()
        {
            UrlHelper.BuildRelativeUri("http://site.com", "sub-page").ShouldBe("http://site.com/sub-page");
            UrlHelper.BuildRelativeUri("http://site.com/", "sub-page").ShouldBe("http://site.com/sub-page");
            UrlHelper.BuildRelativeUri("http://site.com/", "/sub-page").ShouldBe("http://site.com/sub-page");
        }

        [Fact]
        public void ShouldDetermineEqualHosts()
        {
            UrlHelper.EqualHosts("http://site.com", "http://site.com").ShouldBeTrue();
            UrlHelper.EqualHosts("http://SITE.com", "http://site.com").ShouldBeTrue();
            UrlHelper.EqualHosts("https://site.com", "http://site.com").ShouldBeTrue();
            UrlHelper.EqualHosts("https://site.com", "http://site.com/page").ShouldBeTrue();
            UrlHelper.EqualHosts("https://site1.com", "http://site.com").ShouldBeFalse();
            UrlHelper.EqualHosts("https://site.com", "http://site.net").ShouldBeFalse();
        }

        [Fact]
        public void ShouldDetermineIfItIsExternalLink()
        {
            UrlHelper.IsAbsoluteUrl("http://site.com").ShouldBeTrue();
            UrlHelper.IsAbsoluteUrl("https://site.com").ShouldBeTrue();
            UrlHelper.IsAbsoluteUrl("//site.com").ShouldBeTrue();
        }

        [Fact]
        public void ShouldExtractRoot()
        {
            UrlHelper.ExtractRoot("http://site.com").ShouldBe("http://site.com");
            UrlHelper.ExtractRoot("http://site.com/sub-page").ShouldBe("http://site.com");
            UrlHelper.ExtractRoot("http://site.com/sub-page/sub-sub-page").ShouldBe("http://site.com");
        }

        [Fact]
        public void ShouldGetPartialUrl()
        {
            UrlHelper.GetPartialUrl("http://site.com").ShouldBe(string.Empty);
            UrlHelper.GetPartialUrl("http://site.com/#").ShouldBe("/#");
            UrlHelper.GetPartialUrl("http://site.com/#test").ShouldBe("/#test");
            UrlHelper.GetPartialUrl("http://site.com/test#part").ShouldBe("#part");
        }

        [Fact]
        public void ShouldNormalizeUrl()
        {
            UrlHelper.NormalizeUrl("http://site.com").ShouldBe("http://site.com");
            UrlHelper.NormalizeUrl("http://site.com/#").ShouldBe("http://site.com");
            UrlHelper.NormalizeUrl("http://site.com/#test").ShouldBe("http://site.com");
            UrlHelper.NormalizeUrl("//site.com").ShouldBe("https://site.com");
            UrlHelper.NormalizeUrl("https://site.com/").ShouldBe("https://site.com");
            UrlHelper.NormalizeUrl("//").ShouldBeNull();
            UrlHelper.NormalizeUrl(";").ShouldBeNull();
        }
    }
}