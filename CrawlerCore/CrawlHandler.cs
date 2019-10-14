using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Logic;

namespace Crawler
{
    public static class CrawlHandler
    {
        static CrawlHandler()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static async Task Process(Configuration configuration)
        {
            await Process(configuration, CancellationToken.None).ConfigureAwait(false);
        }

        public static async Task Process(Configuration configuration, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(configuration.DestinationFolder))
            {
                var tmpPath = Path.GetTempPath();
                tmpPath = tmpPath.Remove(tmpPath.LastIndexOf(Path.DirectorySeparatorChar));
                configuration.DestinationFolder = configuration.DestinationFolder.Replace("${TempPath}", tmpPath);
            }

            var loader = new FileLoader(token);
            var mapper = new UrlMapper(configuration);
            var builder = new ItemBuilder(configuration, mapper, loader, token);
            var root = await builder.Build().ConfigureAwait(false);

            await ItemWriter.Write(root, mapper).ConfigureAwait(false);
        }
    }
}