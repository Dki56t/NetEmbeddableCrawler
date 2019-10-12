using Crawler.Logic;
using Xunit;

namespace Tests.UnitTests
{
    public class HtmlHelperTest
    {
        [Fact]
        public void TestNodeTypeResolveBinary()
        {
            var url = "https://subdomain.domain.com";
            var urlJpeg = "https://subdomain.domain.com/picture.jpeg";

            Assert.Equal(NodeType.Binary, HtmlHelper.ResolveType("a", urlJpeg));
            Assert.Equal(NodeType.Binary, HtmlHelper.ResolveType("img", url));
        }

        [Fact]
        public void TestNodeTypeResolveHtml()
        {
            var url = "https://subdomain.domain.com";
            var urlPartial = "https://subdomain.domain.com/#23";
            var urlNumbers = "https://subdomain.domain.com/1.2.5";

            Assert.Equal(NodeType.Html, HtmlHelper.ResolveType("a", url));
            Assert.Equal(NodeType.Html, HtmlHelper.ResolveType("a", urlPartial));
            Assert.Equal(NodeType.Html, HtmlHelper.ResolveType("a", urlNumbers));
        }

        [Fact]
        public void TestNodeTypeResolvePartial()
        {
            var urlSubPartial = "#23";

            Assert.Equal(NodeType.Partial, HtmlHelper.ResolveType("a", urlSubPartial));
        }

        [Fact]
        public void TestNodeTypeResolveText()
        {
            var urlText = "https://subdomain.domain.com/text.txt";
            var urlCss = "css/site.css";

            Assert.Equal(NodeType.Text, HtmlHelper.ResolveType("a", urlText));
            Assert.Equal(NodeType.Text, HtmlHelper.ResolveType("a", urlCss));
        }
    }
}