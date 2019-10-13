using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Crawler.Logic
{
    internal static class ItemWriter
    {
        public static async Task Write(Item item, IUrlMapper mapper)
        {
            var tasks = new List<Task>();
            Write(item, mapper, tasks, new ConcurrentDictionary<string, Item>());

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private static void Write(Item item, IUrlMapper mapper, ICollection<Task> tasks,
            ConcurrentDictionary<string, Item> paths)
        {
            var path = mapper.GetPath(item.Uri);
            var directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(directoryPath ?? throw new InvalidOperationException($"Invalid path {path}"));

            if (!paths.TryAdd(path, item))
                throw new InvalidOperationException("Duplicated paths writes");

            if (item.ByteContent != null)
                tasks.Add(Task.Run(async () =>
                {
                    await using var stream = File.Create(path);
                    await stream.WriteAsync(item.ByteContent, 0, item.ByteContent.Length).ConfigureAwait(false);
                    await stream.FlushAsync().ConfigureAwait(false);
                }));
            else
                tasks.Add(Task.Run(async () =>
                {
                    await using var writer = File.CreateText(path);
                    await writer.WriteAsync(item.Content).ConfigureAwait(false);
                    await writer.FlushAsync().ConfigureAwait(false);
                }));

            foreach (var i in item.GetSubItems())
                Write(i, mapper, tasks, paths);
        }
    }
}