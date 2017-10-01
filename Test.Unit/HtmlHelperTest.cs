using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Unit
{
    [TestClass]
    public class HtmlHelperTest
    {
        [TestMethod]
        public void TestNodeTypeResolveHtml()
        {
            string url = "https://subdomain.domain.com";
            string urlPartial = "https://subdomain.domain.com/#23";
            string urlNumbers = "https://subdomain.domain.com/1.2.5";

            Assert.AreEqual(HtmlHelper.ResolveType("a", url), NodeType.Html);
            Assert.AreEqual(HtmlHelper.ResolveType("a", urlPartial), NodeType.Html);
            Assert.AreEqual(HtmlHelper.ResolveType("a", urlNumbers), NodeType.Html);
        }

        [TestMethod]
        public void TestNodeTypeResolvePartial()
        {
            string urlSubPartial = "#23";
            
            Assert.AreEqual(HtmlHelper.ResolveType("a", urlSubPartial), NodeType.Partial);
        }

        [TestMethod]
        public void TestNodeTypeResolveBinary()
        {
            string url = "https://subdomain.domain.com";
            string urlJpeg = "https://subdomain.domain.com/picture.jpeg";

            Assert.AreEqual(HtmlHelper.ResolveType("a", urlJpeg), NodeType.Binary);
            Assert.AreEqual(HtmlHelper.ResolveType("img", url), NodeType.Binary);
        }

        [TestMethod]
        public void TestNodeTypeResolveText()
        {
            string urlText = "https://subdomain.domain.com/text.txt";
            string urlCss = "css/site.css";
            
            Assert.AreEqual(HtmlHelper.ResolveType("a", urlText), NodeType.Text);
            Assert.AreEqual(HtmlHelper.ResolveType("a", urlCss), NodeType.Text);
        }
    }
}
