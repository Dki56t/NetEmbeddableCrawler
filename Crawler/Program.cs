using System.IO;
using Crawler.Logic;

namespace Crawler
{
    class Program
    {
        static void Main()
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

            //Console.ReadLine();
            ItemWriter.Write(root, mapper);
        }
    }
}
