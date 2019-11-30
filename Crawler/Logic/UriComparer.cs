using System;
using System.Collections.Generic;

namespace Crawler.Logic
{
    internal class UriComparer : IEqualityComparer<Uri>
    {
        private const UriComponents Components = UriComponents.Host | UriComponents.Path | UriComponents.Query;
        private const UriFormat Format = UriFormat.SafeUnescaped;
        private const StringComparison Comparison = StringComparison.InvariantCultureIgnoreCase;

        public bool Equals(Uri x, Uri y)
        {
            return Uri.Compare(x, y, Components, Format, Comparison) == 0;
        }

        public int GetHashCode(Uri obj)
        {
            var components = obj.GetComponents(Components, Format);
            return components.GetHashCode(Comparison);
        }
    }
}