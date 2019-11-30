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
            UrlHelper.EqualHosts("http://site.com".AsUri(), "http://site.com".AsUri()).ShouldBeTrue();
            UrlHelper.EqualHosts("http://SITE.com".AsUri(), "http://site.com".AsUri()).ShouldBeTrue();
            UrlHelper.EqualHosts("https://site.com".AsUri(), "http://site.com".AsUri()).ShouldBeTrue();
            UrlHelper.EqualHosts("https://site.com".AsUri(), "http://site.com/page".AsUri()).ShouldBeTrue();
            UrlHelper.EqualHosts("https://site1.com".AsUri(), "http://site.com".AsUri()).ShouldBeFalse();
            UrlHelper.EqualHosts("https://site.com".AsUri(), "http://site.net".AsUri()).ShouldBeFalse();
        }

        [Fact]
        public void ShouldDetermineIfItIsAbsoluteOfFileLink()
        {
            UrlHelper.IsAbsoluteFileOrHttpUri("http://site.com").ShouldBeTrue();
            UrlHelper.IsAbsoluteFileOrHttpUri("https://site.com").ShouldBeTrue();
            UrlHelper.IsAbsoluteFileOrHttpUri("//site.com").ShouldBeTrue();
            UrlHelper
                .IsAbsoluteFileOrHttpUri("data:image/png;base64," +
                               "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=")
                .ShouldBeFalse();
            UrlHelper.IsAbsoluteFileOrHttpUri("javascript:void(0)").ShouldBeFalse();
        }

        [Fact]
        public void ShouldExtractRoot()
        {
            UrlHelper.ExtractRoot("http://site.com".AsUri()).ShouldBe("http://site.com".AsUri());
            UrlHelper.ExtractRoot("http://site.com/sub-page".AsUri()).ShouldBe("http://site.com".AsUri());
            UrlHelper.ExtractRoot("http://site.com/sub-page/sub-sub-page".AsUri()).ShouldBe("http://site.com".AsUri());
        }

        [Fact]
        public void ShouldGetFragmentComponent()
        {
            UrlHelper.GetFragmentComponent("http://site.com".AsUri()).ShouldBe(string.Empty);
            UrlHelper.GetFragmentComponent("http://site.com/#".AsUri()).ShouldBe(string.Empty);
            UrlHelper.GetFragmentComponent("http://site.com/#test".AsUri()).ShouldBe("test");
            UrlHelper.GetFragmentComponent("http://site.com/test#part".AsUri()).ShouldBe("part");
        }

        [Fact]
        public void ShouldNormalizeUrl()
        {
            UrlHelper.NormalizeUrl("http://site.com").ShouldBe("http://site.com".AsUri());
            UrlHelper.NormalizeUrl("http://site.com/#").ShouldBe("http://site.com".AsUri());
            UrlHelper.NormalizeUrl("http://site.com/#test").ShouldBe("http://site.com".AsUri());
            UrlHelper.NormalizeUrl("//site.com").ShouldBe("https://site.com".AsUri());
            UrlHelper.NormalizeUrl("https://site.com/").ShouldBe("https://site.com".AsUri());
            UrlHelper.NormalizeUrl("//").ShouldBeNull();
            UrlHelper.NormalizeUrl(";").ShouldBeNull();
        }
    }
}