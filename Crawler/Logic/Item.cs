using System;
using System.Collections.Generic;
using System.Linq;

namespace Crawler.Logic
{
    /// <summary>
    /// Represent an item (some element) of a web page
    /// </summary>
    internal class Item
    {
        private Item _parent;
        private HashSet<Item> Items { get; }
        public byte[] ByteContent { get; }
        public string Content { get; }
        public string Path { get; }

        public Item(string content, string path)
        {
            Content = content;
            Path = path;
            Items = new HashSet<Item>();
        }

        public Item(byte[] content, string path)
        {
            ByteContent = content;
            Path = path;
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
            if(item._parent != null)
                throw new InvalidOperationException("Item can't have more than one parent");

            item._parent = this;
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

        public Item GetRoot()
        {
            return GetRoot(this);
        }

        private Item GetRoot(Item item)
        {
            if (item._parent != null)
                return GetRoot(item._parent);
            return item;
        }
    }
}
