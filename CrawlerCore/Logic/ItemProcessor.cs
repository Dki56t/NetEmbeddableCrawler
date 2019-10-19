using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Projections;

namespace Crawler.Logic
{
    internal sealed class ItemProcessor
    {
        private readonly CancellationToken _cancellationToken;
        private readonly Configuration _configuration;
        private readonly ConcurrentDictionary<int, int> _depthDictionary;
        private readonly IFileLoader _fileLoader;
        private readonly IItemWriter _itemWriter;
        private readonly IItemParser _parser;

        public ItemProcessor(IFileLoader fileLoader, IItemParser parser, IItemWriter itemWriter,
            Configuration configuration, CancellationToken cancellationToken)
        {
            if (configuration.Depth < 0)
                throw new InvalidOperationException($"Unexpected depth ({configuration.Depth})");

            _fileLoader = fileLoader;
            _parser = parser;
            _itemWriter = itemWriter;
            _configuration = configuration;
            _cancellationToken = cancellationToken;
            _depthDictionary = new ConcurrentDictionary<int, int>();
        }

        public async Task Run()
        {
            _cancellationToken.ThrowIfCancellationRequested();

            var rootLink = UrlHelper.NormalizeUrl(_configuration.RootLink);
            if (string.IsNullOrEmpty(rootLink))
                throw new InvalidOperationException("Invalid root link");

            var item = new Item(rootLink, ItemType.Html, UrlHelper.ExtractRoot(rootLink));

            await WaitBeforeGoingDeeper(_configuration.Depth, 1).ConfigureAwait(false);
            await Process(item, null, _configuration.Depth).ConfigureAwait(false);
        }

        private async Task Process(Item item, WalkContext context, int depth)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            // Load.
            await Task.Yield();
            item = await Load(item).ConfigureAwait(false);

            // TODO make a list of unloaded items?
            if (item.IsEmpty)
                return;

            // ParseAndUpdateContent and update content.
            await Task.Yield();
            var parsingResult = ParseAndUpdateContent(item, depth == 0, context);

            // Save.
            await Task.Yield();
            var tasks = new List<Task>
            {
                Save(item)
            };

            ReleaseProcessBranch(depth);

            if (parsingResult?.DeeperItems != null && depth > 0)
            {
                await WaitBeforeGoingDeeper(depth - 1, parsingResult.DeeperItems.Count).ConfigureAwait(false);
                tasks.AddRange(parsingResult.DeeperItems.Select(u => Process(u, parsingResult.Context, depth - 1)));
            }

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

        private ParsingResult ParseAndUpdateContent(Item item, bool leaf, WalkContext context)
        {
            return _parser.ParseAndUpdateContent(item, !leaf, context);
        }

        private async Task Save(Item item)
        {
            await _itemWriter.Write(item).ConfigureAwait(false);
        }

        /// <summary>
        ///     It should not process deeper level before it load all urls from current.
        ///     Otherwise it would be impossible to get, whether a particular url accessible from
        ///     higher levels (and should be potentially mapped to file system) or not.
        ///     Process will fail in case of unhandled exception so it is not necessary to wrap locking
        ///     into try/finally block
        /// </summary>
        private async Task WaitBeforeGoingDeeper(int newDepth, int count)
        {
            while (true)
            {
                var allReleased = true;
                for (var current = newDepth + 1; current <= _configuration.Depth; current++)
                    allReleased = allReleased && _depthDictionary[current] == 0;

                if (allReleased)
                    break;

                await Task.Delay(10, _cancellationToken).ConfigureAwait(false);
            }

            _depthDictionary.AddOrUpdate(newDepth, count, (key, value) => value + count);
        }

        private void ReleaseProcessBranch(int depth)
        {
            if (_depthDictionary.TryAdd(depth, 0))
                throw new InvalidOperationException("Released not locked branch");

            _depthDictionary.AddOrUpdate(depth, 0, (key, value) => value - 1);
        }
    }
}