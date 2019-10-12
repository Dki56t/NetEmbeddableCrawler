using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Crawler.Logic
{
    internal class ItemBuilder
    {
        private readonly Configuration _cfg;
        private readonly UrlMapper _mapper;

        public ItemBuilder(Configuration cfg, UrlMapper mapper)
        {
            _cfg = cfg;
            _mapper = mapper;
        }

        public async Task<Item> Build(FileLoader loader)
        {
            var rootLink = UrlHelper.NormalizeUrl(_cfg.RootLink);
            var htmlDoc = await LoadDocument(loader, rootLink);
            if (htmlDoc == null) return null;
            var item = new Item(htmlDoc.DocumentNode.OuterHtml, rootLink);
            _mapper.GetPath(item.Uri, NodeType.Html);
            await Walk(item, htmlDoc.DocumentNode, loader, new Dictionary<string, Processing>
                {
                    {
                        rootLink, new Processing
                        {
                            Owner = htmlDoc.DocumentNode,
                            ContentIsProcessed = true
                        }
                    }
                }, rootLink, _cfg.Depth);

            return item;
        }

        private async Task Walk(Item item, HtmlNode node, FileLoader loader,
            Dictionary<string, Processing> processedUrls, string root, int depth)
        {
            if (depth == 0)
                return;

            var links = PreprocessNodeAndGetLink(node, processedUrls, root);

            foreach (var link in links)
            {
                var uri = PrepareUri(link.Value, root);
                var partialPart = UrlHelper.GetPartialUrl(uri);
                var newRoot = UrlHelper.ExtractRoot(uri);

                //check crawling is allow
                if (!CrawlingIsAllowed(root, newRoot)) continue;

                //check continue processing
                var type = HtmlHelper.ResolveType(link.OwnerNode.Name, link.Value);
                if (UrlHelper.IsExternalLink(link.Value) || type == NodeType.Html)
                    link.Value = $"{_mapper.GetPath(uri, type)}{partialPart}";
                if (processedUrls[uri].Owner != node || processedUrls[uri].ContentIsProcessed)
                    continue;
                processedUrls[uri].ContentIsProcessed = true;

                //walking
                switch (type)
                {
                    case NodeType.Html:
                        var doc = await LoadDocument(loader, uri);
                        if (doc == null) continue; //if we can't parse document just skip link
                        var newItem = ProcessContent(item, doc.DocumentNode.OuterHtml, null, link, type, uri);
                        await Walk(newItem, doc.DocumentNode, loader, processedUrls, newRoot, depth - 1);
                        break;
                    case NodeType.Text:
                        ProcessContent(item, await loader.LoadString(uri), null, link, type, uri);
                        break;
                    case NodeType.Binary:
                        ProcessContent(item, null, await loader.LoadBytes(uri), link, type, uri);
                        break;
                }

                //NodeType.Mail just ignored, NodeType.Partial will be normalized and processed as html
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

            //replace url with path in filesystem
            var path = _mapper.GetPath(newItem.Uri, type);
            if (UrlHelper.IsExternalLink(link.Value) || type == NodeType.Html)
                link.Value = $"{path}{UrlHelper.GetPartialUrl(uri)}";

            return newItem;
        }

        private List<HtmlAttribute> PreprocessNodeAndGetLink(HtmlNode node,
            Dictionary<string, Processing> processedUrls,
            string root)
        {
            var links = new List<HtmlAttribute>();

            foreach (var element in node.Descendants()
                .SelectMany(x => x.Attributes).ToArray())
            {
                //find all links
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

                //remove crossorigin for correct work in chrome
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

        //Load html document from url
        private async Task<HtmlDocument> LoadDocument(FileLoader loader, string url)
        {
            var pageStr = await loader.LoadString(url);
            if (string.IsNullOrEmpty(pageStr))
                return null;

            var doc = new HtmlDocument();
            doc.LoadHtml(pageStr);

            return doc;
        }

        private string PrepareUri(string url, string root)
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