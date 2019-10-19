using System.Threading.Tasks;

namespace Crawler.Logic
{
    public interface IItemWriter
    {
        Task Write(Item item);
    }
}