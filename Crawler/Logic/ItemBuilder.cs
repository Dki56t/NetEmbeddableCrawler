using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;

namespace Crawler.Logic
{
    internal class ItemBuilder
    {
        private readonly Configuration _cfg;

        public ItemBuilder(Configuration cfg)
        {
            _cfg = cfg;
        }

        public Item Build()
        {
            Item item = null;
            using (FileLoader loader = new FileLoader())
            {
                var htmlDoc = LoadDocument(loader, _cfg.RootLink);
                if (htmlDoc != null)
                {
                    item = new Item(htmlDoc.DocumentNode.OuterHtml, _cfg.RootLink);
                    Walk(item, htmlDoc.DocumentNode, loader, new HashSet<string> {_cfg.RootLink}, _cfg.RootLink,
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
                var type = ResolveType(link.OwnerNode, link.Value);
                var subPath = UrlHelper.IsExternalLink(link.Value) ? link.Value : BuildRelativeUri(root, link.Value);
                if(processedUrls.Contains(subPath))
                    continue;

                if (type == NodeType.Html)
                {
                    var doc = LoadDocument(loader, subPath);
                    if(doc == null)
                        continue; //if we can't parse document just skip link

                    var descItem = new Item(doc.DocumentNode.OuterHtml, subPath);
                    item.AddItem(descItem);
                    Walk(descItem, doc.DocumentNode, loader, processedUrls, ExtractRoot(subPath), depth - 1);
                }
                else if (type == NodeType.Text)
                {
                    item.AddItem(new Item(loader.LoadString(subPath), subPath));
                }
                else if(type == NodeType.Binary)
                {
                    item.AddItem(new Item(loader.LoadBytes(subPath), subPath));
                }
                //type == NodeType.Partial and NodeType.Mail just ignored

                processedUrls.Add(subPath);
            }
        }

        private string BuildRelativeUri(string root, string relative)
        {
            if (root.EndsWith("/") || relative.StartsWith("/"))
                return $"{root}{relative}";
            return $"{root}/{relative}";
        }

        private HtmlDocument LoadDocument(FileLoader loader, string url)
        {
            if (url.StartsWith("//"))
                url = url.Insert(0, "https:");

            string pageStr = loader.LoadString(url);
            if (string.IsNullOrEmpty(pageStr))
                return null;

            var doc = new HtmlDocument();
            doc.LoadHtml(pageStr);

            return doc;
        }

        private string ExtractRoot(string url)
        {
            var uri = new Uri(url);
            return uri.GetLeftPart(UriPartial.Authority);
        }

        private NodeType ResolveType(HtmlNode node, string url)
        {
            if (Constant.BinaryNodes.Contains(node.Name))
                return NodeType.Binary;

            var ext = Path.GetExtension(url);
            if (url.StartsWith("#"))
                return NodeType.Partial;
            if (url.StartsWith("mailto"))
                return NodeType.Mail;
            if (Constant.TxtFileExtensions.Contains(ext))
                return NodeType.Text;
            if (string.IsNullOrEmpty(ext))
                return NodeType.Html;
            return NodeType.Binary;
        }

        public enum NodeType
        {
            Html,
            Text,
            Binary,
            Partial,
            Mail
        }
    }
}
