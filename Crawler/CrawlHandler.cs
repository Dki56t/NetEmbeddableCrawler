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

        public static async Task<ProcessingResult> ProcessAsync(Configuration configuration)
        {
            return await ProcessAsync(configuration, CancellationToken.None).ConfigureAwait(false);
        }

        public static async Task<ProcessingResult> ProcessAsync(Configuration configuration, CancellationToken token)
        {
            using var fileLoader = new FileLoader(token);
            var mapper = new UrlMapper(configuration);
            var parser = new ItemParser(mapper, configuration.Mode);
            var writer = new ItemWriter(mapper);
            var processor = new ItemProcessor(fileLoader, parser, writer, configuration, token);

            await processor.RunAsync().ConfigureAwait(false);

            return new ProcessingResult(fileLoader.FailedUrls);
        }
    }
}