using Crawler.Projections;

namespace Crawler.Logic
{
    internal interface IItemParser
    {
        ParsingResult? ParseAndUpdateContent(Item item, bool allowUrlMappingCreation, WalkContext? context = null);
    }
}