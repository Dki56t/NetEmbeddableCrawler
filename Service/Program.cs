using System.Threading.Tasks;
using Crawler;

namespace Service
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await CrawlHandler.Process(new Configuration
            {
                RootLink = "http://html-agility-pack.net/",
                Depth = 2,
                DestinationFolder = "${TempPath}\\TestFileWrite",
                FullTraversal = true
            });
        }
    }
}