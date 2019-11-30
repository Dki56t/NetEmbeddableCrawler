using System;
using System.Collections.Concurrent;

namespace Crawler.Logic
{
    internal sealed class WalkContext
    {
        private readonly ConcurrentDictionary<Uri, byte> _processedUris;

        public WalkContext(Uri rootUri)
        {
            _processedUris = new ConcurrentDictionary<Uri, byte>(new UriComparer());
            if (!_processedUris.TryAdd(rootUri, 0))
                throw new InvalidOperationException("Invalid initialization of walking context");
        }

        public bool TryRequestContentProcessing(Uri uri)
        {
            return _processedUris.TryAdd(uri, 0);
        }
    }
}