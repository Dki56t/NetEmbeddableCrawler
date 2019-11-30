using Crawler.Logic;
using Shouldly;
using Xunit;

namespace Tests.UnitTests
{
    public class HtmlHelperTests
    {
        [Fact]
        public void ShouldResolveBinaryNodeType()
        {
            const string url = "https://subdomain.domain.com";
            const string urlJpeg = "https://subdomain.domain.com/picture.jpeg";

            HtmlHelper.ResolveType("img", url).ShouldBe(NodeType.Binary);
            HtmlHelper.ResolveType("a", urlJpeg).ShouldBe(NodeType.Binary);
        }

        [Fact]
        public void ShouldResolveFragmentedNodeType()
        {
            const string urlSubFragmented = "#23";

            HtmlHelper.ResolveType("a", urlSubFragmented).ShouldBe(NodeType.Fragmented);
        }

        [Fact]
        public void ShouldResolveHtmlNodeType()
        {
            const string url = "https://subdomain.domain.com";
            const string urlFragmented = "https://subdomain.domain.com/#23";
            const string urlNumbers = "https://subdomain.domain.com/1.2.5";

            HtmlHelper.ResolveType("a", url).ShouldBe(NodeType.Html);
            HtmlHelper.ResolveType("a", urlFragmented).ShouldBe(NodeType.Html);
            HtmlHelper.ResolveType("a", urlNumbers).ShouldBe(NodeType.Html);
        }

        [Fact]
        public void ShouldResolveTextNodeType()
        {
            const string urlText = "https://subdomain.domain.com/text.txt";
            const string urlCss = "css/site.css";

            HtmlHelper.ResolveType("a", urlText).ShouldBe(NodeType.Text);
            HtmlHelper.ResolveType("a", urlCss).ShouldBe(NodeType.Text);
        }
    }
}