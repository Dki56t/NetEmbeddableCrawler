using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Crawler.Projections;

namespace Crawler.Logic
{
    internal sealed class ItemWriter : IItemWriter
    {
        private readonly IUriMapper _uriMapper;
        private readonly ConcurrentDictionary<string, byte> _wroteFiles;

        public ItemWriter(IUriMapper uriMapper)
        {
            _uriMapper = uriMapper;
            _wroteFiles = new ConcurrentDictionary<string, byte>();
        }

        public async Task WriteAsync(Item item)
        {
            if (item.Content != null && item.ByteContent != null)
                throw new InvalidOperationException(
                    $"Ambiguity in Item content. Only one of them should be filled ({item.Uri}).");

            var path = _uriMapper.CreatePath(item.Uri);
            if (string.IsNullOrEmpty(path))
                throw new InvalidOperationException($"Can not write item due to incorrect mapping (uri={item.Uri})");

            var directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(directoryPath ?? throw new InvalidOperationException($"Invalid path {path}"));

            if (!_wroteFiles.TryAdd(path, 0))
                throw new InvalidOperationException("Duplicated paths writes");

            if (item.ByteContent != null)
            {
                await using var stream = File.Create(path);
                await stream.WriteAsync(item.ByteContent, 0, item.ByteContent.Length).ConfigureAwait(false);
                await stream.FlushAsync().ConfigureAwait(false);
            }
            else
            {
                await using var writer = File.CreateText(path);
                await writer.WriteAsync(item.Content).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}