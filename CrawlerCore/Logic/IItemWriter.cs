using System.Threading.Tasks;
using Crawler.Projections;

namespace Crawler.Logic
{
    public interface IItemWriter
    {
        Task Write(Item item);
    }
}