using System.IO;
using System.Threading.Tasks;
using Crawler;
using Xunit;

namespace Tests.IntegrationTests
{
    public class ItemBuilderTest
    {
        [Fact]
        public async Task TestCrawling()
        {
            await CrawlHandler.Process(new Configuration
            {
                RootLink = "http://google.com/",
                Depth = 1,
                DestinationFolder = Path.Combine(Path.GetTempPath(), "TestFileWrite"),
                FullTraversal = false
            }).ConfigureAwait(false);
        }
    }
}