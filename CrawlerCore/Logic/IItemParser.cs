using Crawler.Projections;

namespace Crawler.Logic
{
    public interface IItemParser
    {
        ParsingResult ParseAndUpdateContent(Item item, bool allowUrlMappingCreation, WalkContext context = null);
    }
}