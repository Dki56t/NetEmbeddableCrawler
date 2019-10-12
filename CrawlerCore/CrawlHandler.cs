using System.IO;
using System.Threading.Tasks;
using Crawler.Logic;

namespace Crawler
{
    public static class CrawlHandler
    {
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
            var builder = new ItemBuilder(configuration, mapper);
            var root = await builder.Build(loader);

            await ItemWriter.Write(root, mapper);
        }
    }
}