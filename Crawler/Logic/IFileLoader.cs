using System.Threading.Tasks;

namespace Crawler.Logic
{
    internal interface IFileLoader
    {
        Task<byte[]> LoadBytesAsync(string url);
        Task<string> LoadStringAsync(string url);
    }
}