using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Crawler.Logic
{
    /// <summary>
    ///     Represent an item (some element) of a web page
    /// </summary>
    internal class Item
    {
        private Item _parent;

        public Item(string content, string uri)
        {
            Content = content;
            Uri = uri;
            Items = new ConcurrentBag<Item>();
        }

        public Item(byte[] content, string uri)
        {
            ByteContent = content;
            Uri = uri;
            Items = new ConcurrentBag<Item>();
        }

        private ConcurrentBag<Item> Items { get; }
        public byte[] ByteContent { get; }
        public string Content { get; private set; }
        public string Uri { get; }

        public void AddItem(Item item)
        {
            if (item._parent != null)
                throw new InvalidOperationException("Item can't have more than one parent");

            item._parent = this;
            Items.Add(item);
        }

        public IReadOnlyCollection<Item> GetSubItems()
        {
            return Items.ToArray();
        }

        public Item GetRoot()
        {
            return GetRoot(this);
        }

        private static Item GetRoot(Item item)
        {
            while (true)
            {
                if (item._parent == null) return item;
                item = item._parent;
            }
        }

        public void UpdateContent(string content)
        {
            Content = content;
        }
    }
}