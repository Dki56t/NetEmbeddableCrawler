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
            var result = await CrawlHandler.ProcessAsync(
                new Configuration("http://google.com/", "${TempPath}\\TestFileWrite")
                {
                    Depth = 2,
                    Mode = TraversalMode.AnyHost
                }).ConfigureAwait(false);

            foreach (var (uri, exception) in result.FailedUris)
                Console.WriteLine($"{uri}: {exception}");
        }
    }
}