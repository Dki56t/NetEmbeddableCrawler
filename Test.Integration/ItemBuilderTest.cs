using System.IO;
using System.Threading.Tasks;
using Crawler;
using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Integration
{
    [TestClass]
    public class ItemBuilderTest
    {
        [TestMethod]
        public async Task TestCrawling()
        {
            FileLoader loader = new FileLoader();
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
            await t.Build(loader);
        }
    }
}