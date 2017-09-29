using System.Collections.Generic;
using System.Linq;

namespace Crawler.Logic
{
    /// <summary>
    /// Represent an item (some element) of a web page
    /// </summary>
    internal class Item
    {
        private HashSet<Item> Items { get; }
        public byte[] ByteContent { get; }
        public string Content { get; }
        public string Path { get; }
        public string SourceUri { get; }

        public Item(string content, string path, string sourceUri)
        {
            Content = content;
            Path = path;
            SourceUri = sourceUri;
            Items = new HashSet<Item>();
        }

        public Item(byte[] content, string path, string sourceUri)
        {
            ByteContent = content;
            Path = path;
            SourceUri = sourceUri;
            Items = new HashSet<Item>();
        }

        public string GetStringContent()
        {
            if (string.IsNullOrEmpty(Content))
                return null;
            return Content;
        }

        public void AddItem(Item item)
        {
            Items.Add(item);
        }
        public void RemoveItem(Item item)
        {
            Items.Remove(item);
        }

        public IReadOnlyCollection<Item> GetSubItems()
        {
            return Items.ToArray();
        }
    }
}
