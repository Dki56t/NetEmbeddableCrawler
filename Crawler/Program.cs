using System;
using System.Configuration;
using System.IO;
using Crawler.Logic;

namespace Crawler
{
    class Program
    {
        static void Main()
        {
            FileLoader loader = new FileLoader();
            var tmpPath = Path.GetTempPath();
            tmpPath = tmpPath.Remove(tmpPath.LastIndexOf(Path.DirectorySeparatorChar));
            
            var cfg = new Configuration
            {
                RootLink = ConfigurationManager.AppSettings["RootLink"],
                Depth = Convert.ToInt16(ConfigurationManager.AppSettings["Depth"]),
                DestinationFolder = ConfigurationManager.AppSettings["DestinationFolder"].Replace("${TempPath}", tmpPath),
                FullTraversal = Convert.ToBoolean(ConfigurationManager.AppSettings["FullTraversal"])
            };

            var mapper = new UrlMapper(cfg);
            var t = new ItemBuilder(cfg, mapper);
            var root = t.Build(loader).Result;

            ItemWriter.Write(root, mapper);
        }
    }
}
