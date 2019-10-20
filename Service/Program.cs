using System;
using System.Threading.Tasks;
using Crawler;
using Crawler.Projections;

namespace Service
{
    internal class Program
    {
        /// <summary>
        ///     An example usage of crawler.
        /// </summary>
        private static async Task Main()
        {
            var result = await CrawlHandler.Process(new Configuration
            {
                RootLink = "http://html-agility-pack.net/",
                Depth = 2,
                DestinationDirectory = "${TempPath}\\TestFileWrite",
                Mode = TraversalMode.AnyHost
            }).ConfigureAwait(false);

            foreach (var (url, exception) in result.FailedUrls)
                Console.WriteLine($"{url}: {exception}");
        }
    }
}