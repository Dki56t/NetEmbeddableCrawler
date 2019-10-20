using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Logic;
using Crawler.Projections;

namespace Crawler
{
    /// <summary>
    ///     Handles requests to parsing.
    /// </summary>
    public static class CrawlHandler
    {
        static CrawlHandler()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static async Task<ProcessingResult> Process(Configuration configuration)
        {
            return await Process(configuration, CancellationToken.None).ConfigureAwait(false);
        }

        public static async Task<ProcessingResult> Process(Configuration configuration, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(configuration.DestinationDirectory))
            {
                var tmpPath = Path.GetTempPath();
                tmpPath = tmpPath.Remove(tmpPath.LastIndexOf(Path.DirectorySeparatorChar));
                configuration.DestinationDirectory = configuration.DestinationDirectory.Replace("${TempPath}", tmpPath);
            }

            using var fileLoader = new FileLoader(token);
            var mapper = new UrlMapper(configuration);
            var parser = new ItemParser(mapper, configuration.Mode);
            var writer = new ItemWriter(mapper);
            var processor = new ItemProcessor(fileLoader, parser, writer, configuration, token);

            await processor.Run().ConfigureAwait(false);

            return new ProcessingResult(fileLoader.FailedUrls);
        }
    }
}