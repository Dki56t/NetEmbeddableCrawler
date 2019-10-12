using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Unit
{
    
    public class HtmlHelperTest
    {
        [Fact]
        public void TestNodeTypeResolveHtml()
        {
            var url = "https://subdomain.domain.com";
            var urlPartial = "https://subdomain.domain.com/#23";
            var urlNumbers = "https://subdomain.domain.com/1.2.5";

            Assert.AreEqual(HtmlHelper.ResolveType("a", url), NodeType.Html);
            Assert.AreEqual(HtmlHelper.ResolveType("a", urlPartial), NodeType.Html);
            Assert.AreEqual(HtmlHelper.ResolveType("a", urlNumbers), NodeType.Html);
        }

        [Fact]
        public void TestNodeTypeResolvePartial()
        {
            var urlSubPartial = "#23";

            Assert.AreEqual(HtmlHelper.ResolveType("a", urlSubPartial), NodeType.Partial);
        }

        [Fact]
        public void TestNodeTypeResolveBinary()
        {
            var url = "https://subdomain.domain.com";
            var urlJpeg = "https://subdomain.domain.com/picture.jpeg";

            Assert.AreEqual(HtmlHelper.ResolveType("a", urlJpeg), NodeType.Binary);
            Assert.AreEqual(HtmlHelper.ResolveType("img", url), NodeType.Binary);
        }

        [Fact]
        public void TestNodeTypeResolveText()
        {
            var urlText = "https://subdomain.domain.com/text.txt";
            var urlCss = "css/site.css";

            Assert.AreEqual(HtmlHelper.ResolveType("a", urlText), NodeType.Text);
            Assert.AreEqual(HtmlHelper.ResolveType("a", urlCss), NodeType.Text);
        }
    }
}