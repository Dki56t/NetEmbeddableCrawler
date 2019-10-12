using System.IO;
using System.Text;
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
            if (!string.IsNullOrEmpty(configuration.DestinationFolder))
            {
                var tmpPath = Path.GetTempPath();
                tmpPath = tmpPath.Remove(tmpPath.LastIndexOf(Path.DirectorySeparatorChar));
                configuration.DestinationFolder = configuration.DestinationFolder.Replace("${TempPath}", tmpPath);
            }

            var loader = new FileLoader();
            var mapper = new UrlMapper(configuration);
            var builder = new ItemBuilder(configuration, mapper, loader);
            var root = await builder.Build().ConfigureAwait(false);

            await ItemWriter.Write(root, mapper).ConfigureAwait(false);
        }
    }
}