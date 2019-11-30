using System;
using Crawler.Logic;

namespace Crawler.Projections
{
    /// <summary>
    ///     Represent an item (some element) of a web page.
    /// </summary>
    internal sealed class Item
    {
        public Item(Uri uri) : this(uri, ItemType.Html, UrlHelper.ExtractRoot(uri))
        {
        }

        public Item(Uri uri, ItemType type, Uri root)
        {
            Uri = uri;
            Type = type;
            Root = root;
        }

        public ItemType Type { get; }
        public Uri Root { get; }
        public byte[]? ByteContent { get; set; }
        public string? Content { get; set; }
        public Uri Uri { get; }
        public bool IsEmpty => (ByteContent == null || ByteContent.Length == 0) && string.IsNullOrWhiteSpace(Content);
    }
}