using System.IO;
using System.Net;
using Crawler.Logic;

namespace Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            FileLoader loader = new FileLoader();
            string testDirectoryPath = Path.Combine(Path.GetTempPath(), "TestFileWrite");
            var cfg = new Configuration
            {
                RootLink = "http://html-agility-pack.net/",
                Depth = 2,
                DestinationFolder = testDirectoryPath,
                FullTraversal = true
            };
            var mapper = new UrlMapper(cfg);
            var t = new ItemBuilder(cfg, mapper);
            var root = t.Build(loader).Result;

            ItemWriter.Write(root, mapper);
        }
    }
}
