using System;
using System.Collections.Generic;
using System.Linq;
using Crawler.Projections;
using HtmlAgilityPack;

namespace Crawler.Logic
{
    internal sealed class ItemParser : IItemParser
    {
        private readonly TraversalMode _mode;
        private readonly IUriMapper _uriMapper;

        public ItemParser(IUriMapper uriMapper, TraversalMode mode)
        {
            _uriMapper = uriMapper;
            _mode = mode;
        }

        public ParsingResult? ParseAndUpdateContent(Item item, bool allowUriMappingCreation,
            WalkContext? context = null)
        {
            if (item.Type != ItemType.Html)
                return null;

            var doc = new HtmlDocument();
            doc.LoadHtml(item.Content);
            var node = doc.DocumentNode;

            if (context == null)
                context = new WalkContext(item.Uri);

            var links = PreprocessNodeAndGetLink(node, item.Root);

            var deeperItems = new List<Item>();
            foreach (var link in links)
            {
                var uri = PrepareUri(link.Value, item.Root);
                if (uri == null)
                    throw new InvalidOperationException("Empty uri can not be processed");

                var newRoot = UrlHelper.ExtractRoot(uri);

                // Check if crawling is allowed.
                if (!CrawlingIsAllowed(item.Root, newRoot)) continue;

                var type = HtmlHelper.ResolveType(link.OwnerNode.Name, link.Value);
                // If crawling should be done, replace uri with path in the file system.
                UpdateLinkUriIfNeeded(uri, link, type, allowUriMappingCreation);

                // Check if processing is necessary.
                if (!context.TryRequestContentProcessing(uri))
                    continue;

                var itemType = ConvertNodeTypeToItemType(type);
                if (itemType != null)
                    deeperItems.Add(new Item(uri, itemType.Value, newRoot));
            }

            item.Content = doc.DocumentNode.OuterHtml;
            return new ParsingResult(deeperItems, context);
        }

        private IEnumerable<HtmlAttribute> PreprocessNodeAndGetLink(HtmlNode node, Uri root)
        {
            var links = new List<HtmlAttribute>();

            var allNestedAttributes = node.Descendants().SelectMany(x => x.Attributes).ToArray();

            foreach (var attribute in allNestedAttributes)
            {
                var uri = PrepareUri(attribute.Value, root);
                if (uri == null)
                    continue;

                // Find all links.
                if (Constant.LinkItems.Contains(attribute.Name))
                    links.Add(attribute);

                // Remove cross-origin for correct work in chrome.
                if (_mode != TraversalMode.SameHostSnapshot &&
                    Constant.CrossOriginItems.Contains(attribute.Name))
                    attribute.Remove();
            }

            return links;
        }

        private void UpdateLinkUriIfNeeded(Uri uri, HtmlAttribute link, NodeType type, bool allowUriMappingCreation)
        {
            if (_mode == TraversalMode.SameHostSnapshot)
                return;

            var fragmentComponent = UrlHelper.GetFragmentComponent(uri);
            if (allowUriMappingCreation)
            {
                link.Value = $"{_uriMapper.CreatePath(uri, type)}{fragmentComponent}";
            }
            else
            {
                var path = _uriMapper.GetPath(uri);
                if (!string.IsNullOrEmpty(path))
                    link.Value = $"{path}{fragmentComponent}";
            }
        }

        private static Uri? PrepareUri(string url, Uri root)
        {
            var uri = UrlHelper.IsAbsoluteUrl(url)
                ? url
                : UrlHelper.BuildRelativeUri(root.OriginalString, url);
            return UrlHelper.NormalizeUrl(uri);
        }

        private bool CrawlingIsAllowed(Uri root, Uri newRoot)
        {
            return _mode == TraversalMode.AnyHost || UrlHelper.EqualHosts(root, newRoot);
        }

        private static ItemType? ConvertNodeTypeToItemType(NodeType type)
        {
            return type switch
            {
                NodeType.Html => ItemType.Html,
                NodeType.Text => ItemType.Text,
                NodeType.Binary => ItemType.Binary,
                NodeType.Fragmented => (ItemType?) null,
                NodeType.Mail => null,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}