using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Crawler.Logic
{
    internal sealed class ItemParser : IItemParser
    {
        private readonly bool _fullTraversal;
        private readonly IUrlMapper _urlMapper;

        public ItemParser(IUrlMapper urlMapper, bool fullTraversal)
        {
            _urlMapper = urlMapper;
            _fullTraversal = fullTraversal;
        }

        public (Item item, List<Item> deeperItems, WalkContext context) Parse(Item item, WalkContext context = null)
        {
            if (item.Type != ItemType.Html)
                return (item, null, null);

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
                if (string.IsNullOrEmpty(uri))
                    throw new InvalidOperationException("Invalid url can not be processed");

                var newRoot = UrlHelper.ExtractRoot(uri);

                // Check if crawling is allowed.
                if (!CrawlingIsAllowed(item.Root, newRoot)) continue;

                // If crawling should be done, replace url with path in the file system.
                var partialPart = UrlHelper.GetPartialUrl(uri);
                var type = HtmlHelper.ResolveType(link.OwnerNode.Name, link.Value);
                if (UrlHelper.IsExternalLink(link.Value) || type == NodeType.Html)
                    link.Value = $"{_urlMapper.CreatePath(uri, type)}{partialPart}";

                // Check if processing is necessary.
                if (!context.TryRequestContentProcessing(uri))
                    continue;

                var itemType = ConvertNodeTypeToItemType(type);
                if (itemType != null)
                    deeperItems.Add(new Item(uri, itemType.Value, newRoot));
            }

            item.Content = doc.DocumentNode.OuterHtml;
            return (item, deeperItems, context);
        }

        private static IEnumerable<HtmlAttribute> PreprocessNodeAndGetLink(HtmlNode node, string root)
        {
            var links = new List<HtmlAttribute>();

            var allNestedAttributes = node.Descendants().SelectMany(x => x.Attributes).ToArray();

            foreach (var attribute in allNestedAttributes)
            {
                var uri = PrepareUri(attribute.Value, root);
                if (string.IsNullOrEmpty(uri))
                    continue;

                // Find all links.
                if (Constant.LinkItems.Contains(attribute.Name)) 
                    links.Add(attribute);

                // Remove cross-origin for correct work in chrome.
                if (Constant.CrossOriginItems.Contains(attribute.Name)) 
                    attribute.Remove();
            }

            return links;
        }

        private static string PrepareUri(string url, string root)
        {
            var uri = UrlHelper.IsExternalLink(url)
                ? url
                : UrlHelper.BuildRelativeUri(root, url);
            return UrlHelper.NormalizeUrl(uri);
        }

        private bool CrawlingIsAllowed(string root, string newRoot)
        {
            return _fullTraversal || UrlHelper.EqualHosts(root, newRoot);
        }

        private static ItemType? ConvertNodeTypeToItemType(NodeType type)
        {
            return type switch
            {
                NodeType.Html => ItemType.Html,
                NodeType.Text => ItemType.Text,
                NodeType.Binary => ItemType.Binary,
                NodeType.Partial => (ItemType?) null,
                NodeType.Mail => null,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}