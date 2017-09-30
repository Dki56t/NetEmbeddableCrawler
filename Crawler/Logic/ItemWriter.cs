using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Crawler.Logic
{
    internal static class ItemWriter
    {
        public static void Write(Item item, UrlMapper mapper)
        {
            var tasks = new List<Task>();
            Write(item, mapper, tasks, new HashSet<string>());

            Task.WaitAll(tasks.ToArray());
        }

        private static void Write(Item item, UrlMapper mapper, List<Task> tasks, HashSet<string> pathes)
        {
            var path = mapper.GetPath(item);
            var directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(directoryPath ?? throw new InvalidOperationException($"Invalid path {path}"));

            if (item.ByteContent != null)
            {
                tasks.Add(Task.Run(() =>
                {
                    if(pathes.Contains(path))
                        throw new InvalidOperationException();
                    pathes.Add(path);
                    using (var stream = File.Create(path))
                    {
                        stream.Write(item.ByteContent, 0, item.ByteContent.Length);
                        stream.Flush();
                    }
                }));
            }
            else
            {
                tasks.Add(Task.Run(() =>
                {
                    if (pathes.Contains(path))
                        throw new InvalidOperationException();
                    pathes.Add(path);
                    using (var writer = File.CreateText(path))
                    {
                        writer.Write(item.Content);
                        writer.Flush();
                    }
                }));
            }

            foreach (var i in item.GetSubItems())
                Write(i, mapper, tasks, pathes);
        }
    }
}
