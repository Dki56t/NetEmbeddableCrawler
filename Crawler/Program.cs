using System.IO;
using System.Net;
using Crawler.Logic;

namespace Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WebClient client = new WebClient())
            {
                FileLoader loader = new FileLoader(client);
                string testDirectoryPath = Path.Combine(Path.GetTempPath(), "TestFileWrite");
                var cfg = new Configuration
                {
                    RootLink = "http://html-agility-pack.net/",
                    Depth = 3,
                    DestinationFolder = testDirectoryPath,
                    FullTraversal = false
                };
                var mapper = new UrlMapper(cfg);
                var t = new ItemBuilder(cfg, mapper);
                var root = t.Build(loader);

                ItemWriter.Write(root, mapper);
            }
        }
    }
}
