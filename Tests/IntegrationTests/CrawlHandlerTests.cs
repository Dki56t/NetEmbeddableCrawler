using System.IO;
using System.Threading.Tasks;
using Crawler;
using Crawler.Projections;
using Xunit;

namespace Tests.IntegrationTests
{
    public class CrawlHandlerTests
    {
        [Fact]
        public async Task ShouldCrawl()
        {
            await CrawlHandler.Process(new Configuration
            {
                RootLink = "http://google.com/",
                Depth = 1,
                DestinationFolder = Path.Combine(Path.GetTempPath(), "TestFileWrite"),
                Mode = TraversalMode.SameHost
            }).ConfigureAwait(false);
        }
    }
}