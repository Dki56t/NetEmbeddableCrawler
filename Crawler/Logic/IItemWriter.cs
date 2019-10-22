using System.Threading.Tasks;
using Crawler.Projections;

namespace Crawler.Logic
{
    internal interface IItemWriter
    {
        Task WriteAsync(Item item);
    }
}