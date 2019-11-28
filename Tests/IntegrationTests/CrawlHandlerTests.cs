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
            await CrawlHandler.ProcessAsync(
                new Configuration("http://google.com/", Path.Combine(Path.GetTempPath(), "TestFileWrite"))
                {
                    Depth = 1,
                    Mode = TraversalMode.SameHost
                }).ConfigureAwait(false);
        }
    }
}