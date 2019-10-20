using System.Threading.Tasks;

namespace Crawler.Logic
{
    internal interface IFileLoader
    {
        Task<byte[]> LoadBytes(string url);
        Task<string> LoadString(string url);
    }
}