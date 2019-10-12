using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Crawler.Logic
{
    internal class ItemBuilder
    {
        private readonly Configuration _cfg;
        private readonly IUrlMapper _mapper;
        private readonly IFileLoader _loader;

        public ItemBuilder(Configuration cfg, IUrlMapper mapper, IFileLoader loader)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            _cfg = cfg;
            _mapper = mapper;
            _loader = loader;
        }

        public async Task<Item> Build()
        {
            var rootLink = UrlHelper.NormalizeUrl(_cfg.RootLink);
            var htmlDoc = await LoadDocument(_loader, rootLink).ConfigureAwait(false);
            if (htmlDoc == null) return null;
            var item = new Item(htmlDoc.DocumentNode.OuterHtml, rootLink);
            _mapper.GetPath(item.Uri, NodeType.Html);
            await Walk(item, htmlDoc.DocumentNode, _loader, new Dictionary<string, Processing>
            {
                {
                    rootLink, new Processing
                    {
                        Owner = htmlDoc.DocumentNode,
                        ContentIsProcessed = true
                    }
                }
            }, rootLink, _cfg.Depth).ConfigureAwait(false);

            return item;
        }

        private async Task Walk(Item item, HtmlNode node, IFileLoader loader,
            IDictionary<string, Processing> processedUrls, string root, int depth)
        {
            if (depth == 0)
                return;

            var links = PreprocessNodeAndGetLink(node, processedUrls, root);

            foreach (var link in links)
            {
                var uri = PrepareUri(link.Value, root);
                var partialPart = UrlHelper.GetPartialUrl(uri);
                var newRoot = UrlHelper.ExtractRoot(uri);

                // Check if crawling is allowed.
                if (!CrawlingIsAllowed(root, newRoot)) continue;

                // Check if processing is necessary.
                var type = HtmlHelper.ResolveType(link.OwnerNode.Name, link.Value);
                if (UrlHelper.IsExternalLink(link.Value) || type == NodeType.Html)
                    link.Value = $"{_mapper.GetPath(uri, type)}{partialPart}";
                if (processedUrls[uri].Owner != node || processedUrls[uri].ContentIsProcessed)
                    continue;
                processedUrls[uri].ContentIsProcessed = true;

                // Walking.
                // Mail links and partial links will be ignored.
                // Partial links will be normalized if it is requested (but will not be visited).
                switch (type)
                {
                    case NodeType.Html:
                        var doc = await LoadDocument(loader, uri).ConfigureAwait(false);
                        var parsingCanBeDone = doc != null;
                        if (!parsingCanBeDone) continue;

                        var newItem = ProcessContent(item, doc.DocumentNode.OuterHtml, null, link, type, uri);
                        await Walk(newItem, doc.DocumentNode, loader, processedUrls, newRoot, depth - 1)
                            .ConfigureAwait(false);
                        break;
                    case NodeType.Text:
                        ProcessContent(item, await loader.LoadString(uri).ConfigureAwait(false), null, link, type, uri);
                        break;
                    case NodeType.Binary:
                        ProcessContent(item, null, await loader.LoadBytes(uri).ConfigureAwait(false), link, type, uri);
                        break;
                    case NodeType.Partial:
                        break;
                    case NodeType.Mail:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            item.UpdateContent(node.OuterHtml);
        }

        private Item ProcessContent(
            Item parent,
            string stringContent, byte[] binaryContent,
            HtmlAttribute link,
            NodeType? type,
            string uri)
        {
            var newItem = binaryContent == null
                ? new Item(stringContent, uri)
                : new Item(binaryContent, uri);
            parent.AddItem(newItem);

            // Replace url with path in file system.
            var path = _mapper.GetPath(newItem.Uri, type);
            if (UrlHelper.IsExternalLink(link.Value) || type == NodeType.Html)
                link.Value = $"{path}{UrlHelper.GetPartialUrl(uri)}";

            return newItem;
        }

        private static IEnumerable<HtmlAttribute> PreprocessNodeAndGetLink(HtmlNode node,
            IDictionary<string, Processing> processedUrls,
            string root)
        {
            var links = new List<HtmlAttribute>();

            foreach (var element in node.Descendants()
                .SelectMany(x => x.Attributes).ToArray())
            {
                // Find all links.
                if (Constant.LinkItems.Contains(element.Name))
                {
                    links.Add(element);
                    var uri = PrepareUri(element.Value, root);
                    if (!processedUrls.ContainsKey(uri))
                        processedUrls.Add(uri, new Processing
                        {
                            Owner = node
                        });
                }

                // Remove cross-origin for correct work in chrome.
                if (Constant.CrossOriginItems.Contains(element.Name))
                    element.Remove();
            }

            return links;
        }

        private bool CrawlingIsAllowed(string root, string newRoot)
        {
            var newRootUri = new Uri(newRoot);
            var currentRootUri = new Uri(root);
            return _cfg.FullTraversal || newRootUri.Host == currentRootUri.Host;
        }

        // Loads html document from url.
        private static async Task<HtmlDocument> LoadDocument(IFileLoader loader, string url)
        {
            var pageStr = await loader.LoadString(url).ConfigureAwait(false);
            if (string.IsNullOrEmpty(pageStr))
                return null;

            var doc = new HtmlDocument();
            doc.LoadHtml(pageStr);

            return doc;
        }

        private static string PrepareUri(string url, string root)
        {
            var uri = UrlHelper.IsExternalLink(url)
                ? url
                : UrlHelper.BuildRelativeUri(root, url);
            return UrlHelper.NormalizeUrl(uri);
        }

        private class Processing
        {
            public HtmlNode Owner { get; set; }
            public bool ContentIsProcessed { get; set; }
        }
    }
}