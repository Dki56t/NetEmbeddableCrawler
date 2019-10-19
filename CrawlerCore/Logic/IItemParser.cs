using System.Collections.Generic;

namespace Crawler.Logic
{
    // todo internal?
    public interface IItemParser
    {
        (Item item, List<Item> deeperItems, WalkContext context) Parse(Item item, WalkContext context = null);
    }
}