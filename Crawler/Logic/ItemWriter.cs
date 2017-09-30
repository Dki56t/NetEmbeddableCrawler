using System;
using System.IO;

namespace Crawler.Logic
{
    internal static class ItemWriter
    {
        public static void Write(Item item, UrlMapper mapper)
        {
            var path = mapper.GetPath(item);
            var directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(directoryPath ?? throw new InvalidOperationException($"Invalid path {path}"));

            if (item.ByteContent != null)
            {
                using (var stream = File.Create(path))
                {
                    stream.Write(item.ByteContent, 0, item.ByteContent.Length);
                    stream.Flush();
                }
            }
            else
            {
                using (var writer = File.CreateText(path))
                {
                    writer.Write(item.Content);
                    writer.Flush();
                }
            }

            foreach (var i in item.GetSubItems())
                Write(i, mapper);
        }
    }
}
