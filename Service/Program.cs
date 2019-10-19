using System.Threading.Tasks;
using Crawler;
using Crawler.Projections;

namespace Service
{
    internal class Program
    {
        private static async Task Main()
        {
            await CrawlHandler.Process(new Configuration
            {
                RootLink = "http://html-agility-pack.net/",
                Depth = 10,
                DestinationFolder = "${TempPath}\\TestFileWrite",
                Mode = TraversalMode.SameHost
            }).ConfigureAwait(false);
        }
    }
}