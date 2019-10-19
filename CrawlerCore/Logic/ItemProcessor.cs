using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Crawler.Logic
{
    internal sealed class ItemProcessor
    {
        private readonly CancellationToken _cancellationToken;
        private readonly Configuration _configuration;
        private readonly IFileLoader _fileLoader;
        private readonly IItemWriter _itemWriter;
        private readonly IItemParser _parser;

        public ItemProcessor(IFileLoader fileLoader, IItemParser parser, IItemWriter itemWriter,
            Configuration configuration, CancellationToken cancellationToken)
        {
            _fileLoader = fileLoader;
            _parser = parser;
            _itemWriter = itemWriter;
            _configuration = configuration;
            _cancellationToken = cancellationToken;
        }

        public async Task Run()
        {
            _cancellationToken.ThrowIfCancellationRequested();

            var rootLink = UrlHelper.NormalizeUrl(_configuration.RootLink);
            if (string.IsNullOrEmpty(rootLink))
                throw new InvalidOperationException("Invalid root link");

            var item = new Item(rootLink, ItemType.Html, UrlHelper.ExtractRoot(rootLink));
            await Process(item, null, _configuration.Depth).ConfigureAwait(false);
        }

        private async Task Process(Item item, WalkContext context, int depth)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            // Load.
            await Task.Yield();
            item = await Load(item).ConfigureAwait(false);

            // TODO make a list of unloaded item?
            if (item.IsEmpty)
                return;

            // Parse.
            await Task.Yield();
            List<Item> deeperItems;
            (item, deeperItems, context) = Parse(item, context);

            // Save.
            await Task.Yield();
            var tasks = new List<Task>
            {
                Save(item)
            };

            if (deeperItems != null && depth > 0)
                tasks.AddRange(deeperItems
                    .Select(u => Process(u, context, depth - 1)));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task<Item> Load(Item item)
        {
            switch (item.Type)
            {
                case ItemType.Html:
                case ItemType.Text:
                    var stringContent = await _fileLoader.LoadString(item.Uri).ConfigureAwait(false);
                    item.Content = stringContent;
                    return item;
                case ItemType.Binary:
                    var byteContent = await _fileLoader.LoadBytes(item.Uri).ConfigureAwait(false);
                    item.ByteContent = byteContent;
                    return item;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown type of the Item ({item.Type})");
            }
        }

        private (Item item, List<Item> deeperItems, WalkContext context) Parse(Item item, WalkContext context)
        {
            return _parser.Parse(item, context);
        }

        private async Task Save(Item item)
        {
            await _itemWriter.Write(item).ConfigureAwait(false);
        }
    }
}