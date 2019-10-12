using System.Threading.Tasks;
using Crawler.Logic;
using Xunit;

namespace Tests.IntegrationTests
{
    public class FileLoaderTest
    {
        [Fact]
        public async Task TestSkipExceptions()
        {
            var loader = new FileLoader();
            await loader.LoadString("https://ru.linkedin.com").ConfigureAwait(false);
        }
    }
}