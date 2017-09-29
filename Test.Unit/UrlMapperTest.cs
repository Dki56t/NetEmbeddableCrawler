using System.IO;
using Crawler;
using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Unit
{
    [TestClass]
    public class UrlMapperTest
    {
        [TestMethod]
        public void TestMapping()
        {
            string testDirectoryPath = Path.Combine(Path.GetTempPath(), "TestFileWrite");
            var item1 = new Item("1", "http://site1/index.html");
            var item11 = new Item("1/internal", "http://site1/internal/internal.html");
            item1.AddItem(item11);
            var item2 = new Item("2", "http://site2/index2.html");
            var item21 = new Item("2/internal", "http://site2/otherinternal/some.html");
            item2.AddItem(item21);
            var item3 = new Item("2", "http://site3/");
            var item31 = new Item("2/internal", "http://site3/test/some.html/");
            item3.AddItem(item31);

            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = testDirectoryPath
            });

            Assert.AreEqual(mapper.GetPath(item1), Path.Combine(testDirectoryPath, "site1\\index.html"));
            Assert.AreEqual(mapper.GetPath(item11), Path.Combine(testDirectoryPath, "site1\\internal\\internal.html"));
            Assert.AreEqual(mapper.GetPath(item2), Path.Combine(testDirectoryPath, "site2\\index2.html"));
            Assert.AreEqual(mapper.GetPath(item21), Path.Combine(testDirectoryPath, "site2\\otherinternal\\some.html"));
            Assert.AreEqual(mapper.GetPath(item3), Path.Combine(testDirectoryPath, "site3\\index.html"));
            Assert.AreEqual(mapper.GetPath(item31), Path.Combine(testDirectoryPath, "site3\\test\\some.html"));
        }
    }
}
