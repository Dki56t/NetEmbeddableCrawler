using Crawler.Logic;
using Xunit;

namespace Tests.UnitTests
{
    public class HtmlHelperTest
    {
        [Fact]
        public void TestNodeTypeResolveBinary()
        {
            const string url = "https://subdomain.domain.com";
            const string urlJpeg = "https://subdomain.domain.com/picture.jpeg";

            Assert.Equal(NodeType.Binary, HtmlHelper.ResolveType("a", urlJpeg));
            Assert.Equal(NodeType.Binary, HtmlHelper.ResolveType("img", url));
        }

        [Fact]
        public void TestNodeTypeResolveHtml()
        {
            const string url = "https://subdomain.domain.com";
            const string urlPartial = "https://subdomain.domain.com/#23";
            const string urlNumbers = "https://subdomain.domain.com/1.2.5";

            Assert.Equal(NodeType.Html, HtmlHelper.ResolveType("a", url));
            Assert.Equal(NodeType.Html, HtmlHelper.ResolveType("a", urlPartial));
            Assert.Equal(NodeType.Html, HtmlHelper.ResolveType("a", urlNumbers));
        }

        [Fact]
        public void TestNodeTypeResolvePartial()
        {
            const string urlSubPartial = "#23";

            Assert.Equal(NodeType.Partial, HtmlHelper.ResolveType("a", urlSubPartial));
        }

        [Fact]
        public void TestNodeTypeResolveText()
        {
            const string urlText = "https://subdomain.domain.com/text.txt";
            const string urlCss = "css/site.css";

            Assert.Equal(NodeType.Text, HtmlHelper.ResolveType("a", urlText));
            Assert.Equal(NodeType.Text, HtmlHelper.ResolveType("a", urlCss));
        }
    }
}