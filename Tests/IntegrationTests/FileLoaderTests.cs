using System.Threading;
using System.Threading.Tasks;
using Crawler.Logic;
using Xunit;

namespace Tests.IntegrationTests
{
    public class FileLoaderTests
    {
        [Fact]
        public async Task ShouldSkipExceptions()
        {
            var loader = new FileLoader(CancellationToken.None);
            await loader.LoadStringAsync("https://ru.linkedin.com").ConfigureAwait(false);
        }
    }
}