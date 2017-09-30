using System.IO;
using System.Net;
using Crawler;
using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Integration
{
    [TestClass]
    public class ItemBuilderTest
    {
        [TestMethod]
        public void TestCrawling()
        {
            using (WebClient client = new WebClient())
            {
                FileLoader loader = new FileLoader(client);
                string testDirectoryPath = Path.Combine(Path.GetTempPath(), "TestFileWrite");
                var cfg = new Configuration
                {
                    RootLink = "http://html-agility-pack.net/",
                    Depth = 3,
                    DestinationFolder = testDirectoryPath,
                    FullTraversal = false
                };
                var mapper = new UrlMapper(cfg);
                var t = new ItemBuilder(cfg, mapper);
                t.Build(loader);
            }
        }
    }
}