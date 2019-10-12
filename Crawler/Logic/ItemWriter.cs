using System;
using System.Collections.Concurrent;
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
            Write(item, mapper, tasks, new ConcurrentDictionary<string, byte>());

            Task.WaitAll(tasks.ToArray());
        }

        private static void Write(Item item, UrlMapper mapper, List<Task> tasks,
            ConcurrentDictionary<string, byte> pathes)
        {
            var path = mapper.GetPath(item.Uri);
            var directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(directoryPath ?? throw new InvalidOperationException($"Invalid path {path}"));

            if (item.ByteContent != null)
                tasks.Add(Task.Run(async () =>
                {
                    do
                    {
                        if (pathes.ContainsKey(path))
                            throw new InvalidOperationException("Duplicated pathes writes");
                    } while (!pathes.TryAdd(path, 1));

                    using (var stream = File.Create(path))
                    {
                        await stream.WriteAsync(item.ByteContent, 0, item.ByteContent.Length);
                        stream.Flush();
                    }
                }));
            else
                tasks.Add(Task.Run(async () =>
                {
                    do
                    {
                        if (pathes.ContainsKey(path))
                            throw new InvalidOperationException("Duplicated pathes writes");
                    } while (!pathes.TryAdd(path, 1));

                    using (var writer = File.CreateText(path))
                    {
                        await writer.WriteAsync(item.Content);
                        writer.Flush();
                    }
                }));

            foreach (var i in item.GetSubItems())
                Write(i, mapper, tasks, pathes);
        }
    }
}