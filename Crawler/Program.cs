using System;
using System.IO;
using System.Linq;
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
                Depth = 3,
                DestinationFolder = testDirectoryPath,
                FullTraversal = false
            };
            var mapper = new UrlMapper(cfg);
            var t = new ItemBuilder(cfg, mapper);
            var root = t.Build(loader).Result;

            //Console.ReadLine();
            ItemWriter.Write(root, mapper);
        }
    }
}
