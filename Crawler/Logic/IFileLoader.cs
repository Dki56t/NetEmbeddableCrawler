using System;
using System.Threading.Tasks;

namespace Crawler.Logic
{
    internal interface IFileLoader
    {
        Task<byte[]?> LoadBytesAsync(Uri uri);
        Task<string?> LoadStringAsync(Uri uri);
    }
}