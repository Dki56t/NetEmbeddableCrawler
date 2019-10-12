using System.IO;
using System.Threading.Tasks;
using Crawler;
using Crawler.Logic;
using Xunit;

namespace Tests.IntegrationTests
{
    public class ItemBuilderTest
    {
        [Fact]
        public async Task TestCrawling()
        {
            var loader = new FileLoader();
            var testDirectoryPath = Path.Combine(Path.GetTempPath(), "TestFileWrite");
            var cfg = new Configuration
            {
                RootLink = "http://google.com/",
                Depth = 1,
                DestinationFolder = testDirectoryPath,
                FullTraversal = false
            };
            var mapper = new UrlMapper(cfg);
            var t = new ItemBuilder(cfg, mapper);
            await t.Build(loader);
        }
    }
}