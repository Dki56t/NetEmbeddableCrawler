using System.IO;
using Crawler;
using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Unit
{
    [TestClass]
    public class HtmlHelperTest
    {
        [TestMethod]
        public void TestNodeTypeResolve()
        {
            string url = "https://subdomain.domain.com";
            string urlPartial = "https://subdomain.domain.com/#23";
            string urlSubPartial = "#23";
            string urlJpeg = "https://subdomain.domain.com/picture.jpeg";
            string urlText = "https://subdomain.domain.com/text.txt";
            string urlCss = "css/site.css";

            Assert.AreEqual(HtmlHelper.ResolveType("a", url), NodeType.Html);
            Assert.AreEqual(HtmlHelper.ResolveType("a", urlPartial), NodeType.Html);

            Assert.AreEqual(HtmlHelper.ResolveType("a", urlSubPartial), NodeType.Partial);

            Assert.AreEqual(HtmlHelper.ResolveType("a", urlJpeg), NodeType.Binary);
            Assert.AreEqual(HtmlHelper.ResolveType("img", url), NodeType.Binary);

            Assert.AreEqual(HtmlHelper.ResolveType("a", urlText), NodeType.Text);
            Assert.AreEqual(HtmlHelper.ResolveType("a", urlCss), NodeType.Text);
        }
    }
}
