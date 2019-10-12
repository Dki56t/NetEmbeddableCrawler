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

            Assert.Equal(HtmlHelper.ResolveType("a", urlJpeg), NodeType.Binary);
            Assert.Equal(HtmlHelper.ResolveType("img", url), NodeType.Binary);
        }

        [Fact]
        public void TestNodeTypeResolveHtml()
        {
            var url = "https://subdomain.domain.com";
            var urlPartial = "https://subdomain.domain.com/#23";
            var urlNumbers = "https://subdomain.domain.com/1.2.5";

            Assert.Equal(HtmlHelper.ResolveType("a", url), NodeType.Html);
            Assert.Equal(HtmlHelper.ResolveType("a", urlPartial), NodeType.Html);
            Assert.Equal(HtmlHelper.ResolveType("a", urlNumbers), NodeType.Html);
        }

        [Fact]
        public void TestNodeTypeResolvePartial()
        {
            var urlSubPartial = "#23";

            Assert.Equal(HtmlHelper.ResolveType("a", urlSubPartial), NodeType.Partial);
        }

        [Fact]
        public void TestNodeTypeResolveText()
        {
            var urlText = "https://subdomain.domain.com/text.txt";
            var urlCss = "css/site.css";

            Assert.Equal(HtmlHelper.ResolveType("a", urlText), NodeType.Text);
            Assert.Equal(HtmlHelper.ResolveType("a", urlCss), NodeType.Text);
        }
    }
}