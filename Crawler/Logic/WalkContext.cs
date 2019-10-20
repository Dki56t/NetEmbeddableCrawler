using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Crawler.Logic
{
    internal sealed class WalkContext
    {
        private readonly ConcurrentDictionary<string, byte> _processedUrls;

        public WalkContext(string rootLink)
        {
            _processedUrls = new ConcurrentDictionary<string, byte>(new UriComparer());
            if (!_processedUrls.TryAdd(rootLink, 0))
                throw new InvalidOperationException("Invalid initialization of walking context");
        }

        public bool TryRequestContentProcessing(string uri)
        {
            return _processedUrls.TryAdd(uri, 0);
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