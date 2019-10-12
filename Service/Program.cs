using System.Threading.Tasks;
using Crawler;

namespace Service
{
    internal class Program
    {
        private static async Task Main()
        {
            await CrawlHandler.Process(new Configuration
            {
                RootLink = "http://html-agility-pack.net/",
                Depth = 2,
                DestinationFolder = "${TempPath}\\TestFileWrite",
                FullTraversal = true
            }).ConfigureAwait(false);
        }
    }
}