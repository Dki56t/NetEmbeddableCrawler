using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Crawler.Logic
{
    internal class ItemBuilder
    {
        private readonly Configuration _cfg;
        private readonly IFileLoader _loader;
        private readonly CancellationToken _token;
        private readonly IUrlMapper _mapper;

        public ItemBuilder(Configuration cfg, IUrlMapper mapper, IFileLoader loader, CancellationToken token)
        {
            _cfg = cfg;
            _mapper = mapper;
            _loader = loader;
            _token = token;
        }

        public async Task<Item> Build()
        {
            _token.ThrowIfCancellationRequested();

            var rootLink = UrlHelper.NormalizeUrl(_cfg.RootLink);
            if (string.IsNullOrEmpty(rootLink))
                throw new InvalidOperationException("Invalid root link");

            var htmlDoc = await LoadDocument(_loader, rootLink).ConfigureAwait(false);
            if (htmlDoc == null)
                throw new InvalidOperationException("Root node can not be processed");

            var item = new Item(htmlDoc.DocumentNode.OuterHtml, rootLink);
            _mapper.CreatePath(item.Uri, NodeType.Html);

            var context = new WalkContext(rootLink, htmlDoc.DocumentNode);
            await Walk(item, htmlDoc.DocumentNode, context, rootLink, _cfg.Depth)
                .ConfigureAwait(false);
            return item;
        }

        private Task Walk(Item item, HtmlNode node, WalkContext context, string root, int depth)
        {
            _token.ThrowIfCancellationRequested();

            if (depth == 0)
                return Task.CompletedTask;

            var childTasks = new List<Task>();
            var links = PreprocessNodeAndGetLink(node, context, root);

            foreach (var link in links)
            {
                var uri = PrepareUri(link.Value, root);
                if (string.IsNullOrEmpty(uri))
                    throw new InvalidOperationException("Invalid url can not be processed");

                var newRoot = UrlHelper.ExtractRoot(uri);

                // Check if crawling is allowed.
                if (!CrawlingIsAllowed(root, newRoot)) continue;

                // If crawling should be done, replace url with path in the file system.
                var partialPart = UrlHelper.GetPartialUrl(uri);
                var type = HtmlHelper.ResolveType(link.OwnerNode.Name, link.Value);
                if (UrlHelper.IsExternalLink(link.Value) || type == NodeType.Html)
                    link.Value = $"{_mapper.CreatePath(uri, type)}{partialPart}";

                // Check if processing is necessary.
                if (!context.TryRequestContentProcessing(uri, node))
                    continue;

                // Walking.
                // Mail links and partial links will be ignored.
                // Partial links will be normalized (but will not be visited).
                switch (type)
                {
                    case NodeType.Html:
                        childTasks.Add(LoadDocument(_loader, uri).ContinueWith(t =>
                        {
                            var doc = t.Result;

                            var loadingFailed = doc == null;
                            if (loadingFailed) return;

                            var newItem = AttachToParent(item, doc.DocumentNode.OuterHtml, uri);

                            Task.Factory.StartNew(async () =>
                                    await Walk(newItem, doc.DocumentNode, context, newRoot, depth - 1)
                                        .ConfigureAwait(false),
                                TaskCreationOptions.AttachedToParent);
                        }, TaskContinuationOptions.AttachedToParent));
                        break;
                    case NodeType.Text:
                        childTasks.Add(_loader.LoadString(uri).ContinueWith(t => AttachToParent(item, t.Result, uri),
                            TaskContinuationOptions.AttachedToParent));
                        break;
                    case NodeType.Binary:
                        childTasks.Add(_loader.LoadBytes(uri).ContinueWith(t => AttachToParent(item, t.Result, uri),
                            TaskContinuationOptions.AttachedToParent));
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
            return Task.WhenAll(childTasks);
        }

        private static Item AttachToParent(Item parent, string content, string uri)
        {
            return AttachToParent(parent, new Item(content, uri));
        }

        private static void AttachToParent(Item parent, byte[] content, string uri)
        {
            AttachToParent(parent, new Item(content, uri));
        }

        private static Item AttachToParent(Item parent, Item newItem)
        {
            parent.AddItem(newItem);
            return newItem;
        }

        private static IEnumerable<HtmlAttribute> PreprocessNodeAndGetLink(HtmlNode node, WalkContext context,
            string root)
        {
            var links = new List<HtmlAttribute>();

            var allNestedAttributes = node.Descendants().SelectMany(x => x.Attributes).ToArray();

            // To run through all node attributes and avoid concurrent creation
            // of ProcessingNode with different Owner but same link, all processing of attributes
            // is in a critical section.
            lock (context)
            {
                foreach (var attribute in allNestedAttributes)
                {
                    var uri = PrepareUri(attribute.Value, root);
                    if (string.IsNullOrEmpty(uri))
                        continue;

                    // Find all links.
                    if (Constant.LinkItems.Contains(attribute.Name))
                    {
                        links.Add(attribute);
                        context.AddUrlIfMetFirstTime(uri, node);
                    }

                    // Remove cross-origin for correct work in chrome.
                    if (Constant.CrossOriginItems.Contains(attribute.Name))
                        attribute.Remove();
                }
            }

            return links;
        }

        private bool CrawlingIsAllowed(string root, string newRoot)
        {
            return _cfg.FullTraversal || UrlHelper.EqualHosts(root, newRoot);
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
    }
}