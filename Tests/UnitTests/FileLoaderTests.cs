using System;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Logic;
using Xunit;

namespace Tests.UnitTests
{
    public class FileLoaderTests
    {
        [Fact]
        public async Task ShouldThrowsIfCancellationRequested()
        {
            using var cts = new CancellationTokenSource();
            var loader = new FileLoader(cts.Token);

            cts.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => loader.LoadString("http://some.com"))
                .ConfigureAwait(false);
        }
    }
}