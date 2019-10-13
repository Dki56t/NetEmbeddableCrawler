using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace Crawler.Logic
{
    internal class WalkContext
    {
        private readonly ConcurrentDictionary<string, ProcessingNode> _processedUrls;

        public WalkContext(string rootLink, HtmlNode documentNode)
        {
            var rootProcessingNode = new ProcessingNode
            {
                Owner = documentNode,
                ContentIsProcessed = true
            };

            _processedUrls = new ConcurrentDictionary<string, ProcessingNode>(new UriComparer());
            if (!_processedUrls.TryAdd(rootLink, rootProcessingNode))
                throw new InvalidOperationException("Invalid initialization of walking context");
        }

        public void AddUrlIfMetFirstTime(string uri, HtmlNode owner)
        {
            _processedUrls.TryAdd(uri, new ProcessingNode
            {
                Owner = owner
            });
        }

        public bool TryRequestContentProcessing(string uri, HtmlNode owner)
        {
            if (_processedUrls[uri].Owner != owner || _processedUrls[uri].ContentIsProcessed)
                return false;

            _processedUrls[uri].ContentIsProcessed = true;
            return true;
        }

        private class ProcessingNode
        {
            public HtmlNode Owner { get; set; }
            public bool ContentIsProcessed { get; set; }
        }

        private class UriComparer : IEqualityComparer<string>
        {
            private const UriComponents Components = UriComponents.Host | UriComponents.Path | UriComponents.Query;
            private const UriFormat Format = UriFormat.SafeUnescaped;
            private const StringComparison Comparison = StringComparison.InvariantCultureIgnoreCase;

            public bool Equals(string x, string y)
            {
                return Uri.Compare(new Uri(x), new Uri(y), Components, Format, Comparison) == 0;
            }

            public int GetHashCode(string obj)
            {
                var uri = new Uri(obj);
                var components = uri.GetComponents(Components, Format);
                return components.GetHashCode(Comparison);
            }
        }
    }
}