using Crawler.Projections;

namespace Crawler.Logic
{
    internal interface IItemParser
    {
        ParsingResult? ParseAndUpdateContent(Item item, bool allowUriMappingCreation, WalkContext? context = null);
    }
}