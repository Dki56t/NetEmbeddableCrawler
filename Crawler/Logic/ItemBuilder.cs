using System;
using System.Collections.Generic;
using System.Linq;
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

        public Item Build(FileLoader loader)
        {
            Item item = null;

            {
                var rootLink = UrlHelper.NormalizeUrl(_cfg.RootLink);
                var htmlDoc = LoadDocument(loader, rootLink);
                if (htmlDoc != null)
                {
                    item = new Item(htmlDoc.DocumentNode.OuterHtml, rootLink);
                    _mapper.GetPath(item);
                    Walk(item, htmlDoc.DocumentNode, loader, new HashSet<string> { rootLink }, rootLink,
                        _cfg.Depth);
                }
            }

            return item;
        }

        private void Walk(Item item, HtmlNode node, FileLoader loader, HashSet<string> processedUrls, string root, int depth)
        {
            if (depth == 0)
                return;

            var links = node.Descendants()
                .SelectMany(x => x.Attributes)
                .Where(x => Constant.LinkItems.Contains(x.Name))
                .ToArray();

            foreach (var link in links)
            {
                var uri = UrlHelper.IsExternalLink(link.Value)
                    ? link.Value
                    : UrlHelper.BuildRelativeUri(root, link.Value);
                uri = UrlHelper.NormalizeUrl(uri);
                var partialPart = UrlHelper.GetPartialUrl(uri);
                var newRoot = UrlHelper.ExtractRoot(uri);

                //check crawling is allow
                if (!CrawlingIsAllowed(root, newRoot))
                {
                    processedUrls.Add(uri);
                    continue; 
                }

                //check already processed
                if (processedUrls.Contains(uri))
                {
                    link.Value = $"{_mapper.GetProcessedPathByUrl(uri)}{partialPart}";
                    continue;
                }

                //walking
                Item newItem = null;
                var type = HtmlHelper.ResolveType(link.OwnerNode.Name, link.Value);
                if (type == NodeType.Html)
                {
                    var doc = LoadDocument(loader, uri);
                    if(doc == null)
                    {
                        processedUrls.Add(uri);
                        continue; //if we can't parse document just skip link
                    }

                    newItem = new Item(doc.DocumentNode.OuterHtml, uri);
                    item.AddItem(newItem);

                    Walk(newItem, doc.DocumentNode, loader, processedUrls, newRoot, depth - 1);
                }
                else if (type == NodeType.Text)
                {
                    newItem = new Item(loader.LoadString(uri), uri);
                    item.AddItem(newItem);
                }
                else if(type == NodeType.Binary)
                {
                    newItem = new Item(loader.LoadBytes(uri), uri);
                    item.AddItem(newItem);
                }
                //type == NodeType.Partial and NodeType.Mail just ignored

                //replace url with path in filesystem
                if (newItem != null)
                {
                    var path = _mapper.GetPath(newItem);
                    link.Value = $"{path}{partialPart}";
                }

                processedUrls.Add(uri);
            }
        }

        private bool CrawlingIsAllowed(string root, string newRoot)
        {
            var newRootUri = new Uri(newRoot);
            var currentRootUri = new Uri(root);
            return _cfg.FullTraversal || newRootUri.Host == currentRootUri.Host;
        }

        private HtmlDocument LoadDocument(FileLoader loader, string url)
        {
            string pageStr = loader.LoadString(url);
            if (string.IsNullOrEmpty(pageStr))
                return null;

            var doc = new HtmlDocument();
            doc.LoadHtml(pageStr);

            return doc;
        }
    }
}
