using System;
using System.Threading;
using System.Threading.Tasks;
using Crawler;
using Crawler.Logic;
using Moq;
using Xunit;

namespace Tests.UnitTests
{
    public class ItemProcessorTests
    {
        public ItemProcessorTests()
        {
            _cfg = new Configuration
            {
                RootLink = "http://site1.com",
                Depth = 3,
                FullTraversal = false
            };
        }

        private const string MainUrl = "http://site1.com";
        private const string SubUrl = "http://site1.com/sub-page";

        private readonly Configuration _cfg;

        private ProcessorTestContainer CreateMocksAndProcessor(Configuration cfg = null,
            CancellationToken? token = null)
        {
            cfg ??= _cfg;

            var fileLoaderMock = new Mock<IFileLoader>();
            var urlMapperMock = new Mock<IUrlMapper>();
            var writerMock = new Mock<IItemWriter>();

            return new ProcessorTestContainer
            {
                LoaderMock = fileLoaderMock,
                UrlMapperMock = urlMapperMock,
                WriterMock = writerMock,
                Processor = new ItemProcessor(fileLoaderMock.Object,
                    new ItemParser(urlMapperMock.Object, cfg.FullTraversal), writerMock.Object, cfg,
                    token ?? CancellationToken.None)
            };
        }

        private class ProcessorTestContainer
        {
            public Mock<IFileLoader> LoaderMock { get; set; }
            public Mock<IUrlMapper> UrlMapperMock { get; set; }
            public Mock<IItemWriter> WriterMock { get; set; }

            public ItemProcessor Processor { get; set; }
        }

        [Fact]
        public async Task ShouldIgnoreItemInCaseOfFailedLoadingOrWithUnsuccessfulResponseCode()
        {
            var mocks = CreateMocksAndProcessor(new Configuration
            {
                RootLink = MainUrl,
                Depth = 1
            });

            mocks.LoaderMock.Setup(x => x.LoadString(MainUrl))
                .ReturnsAsync($"<body><a href=\"{SubUrl}\"> </a></body>");
            mocks.LoaderMock.Setup(x => x.LoadString(SubUrl))
                .ReturnsAsync((string) null);

            await mocks.Processor.Run().ConfigureAwait(false);

            mocks.LoaderMock.Verify(l => l.LoadString(MainUrl), Times.Once);
            mocks.LoaderMock.Verify(l => l.LoadString(SubUrl), Times.Once);
            mocks.WriterMock.Verify(w => w.Write(It.IsAny<Item>()), Times.Once);
            mocks.WriterMock.Verify(w => w.Write(It.Is<Item>(i => i.Uri == MainUrl)), Times.Once);
        }

        [Fact]
        public async Task ShouldLoadSubUrls()
        {
            var mocks = CreateMocksAndProcessor(new Configuration
            {
                RootLink = MainUrl,
                Depth = 1
            });

            var loaderMock = mocks.LoaderMock;
            loaderMock.Setup(x => x.LoadString(MainUrl))
                .ReturnsAsync($"<body><a href=\"{SubUrl}\"> </a></body>");
            loaderMock.Setup(x => x.LoadString(SubUrl))
                .ReturnsAsync("<body></body>");

            await mocks.Processor.Run().ConfigureAwait(false);

            mocks.LoaderMock.Verify(m => m.LoadString(MainUrl), Times.Once);
            mocks.LoaderMock.Verify(m => m.LoadString(SubUrl), Times.Once);
            mocks.WriterMock.Verify(m => m.Write(It.IsAny<Item>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ShouldLoadUrlContentOnlyOnce()
        {
            var mocks = CreateMocksAndProcessor(new Configuration
            {
                RootLink = MainUrl,
                Depth = 1
            });

            mocks.LoaderMock.Setup(x => x.LoadString(MainUrl))
                .ReturnsAsync("<body>" +
                              $"<a href=\"{SubUrl}\"> </a>" +
                              $"<a href=\"{SubUrl}\"> </a></body>");
            mocks.LoaderMock.Setup(x => x.LoadString(SubUrl))
                .ReturnsAsync("<body>" +
                              $"<a href=\"{SubUrl}\"> </a>" +
                              $"<a href=\"{MainUrl}\"> </a>" +
                              "</body>");

            await mocks.Processor.Run().ConfigureAwait(false);

            mocks.LoaderMock.Verify(x => x.LoadString(SubUrl), Times.Once);
            mocks.LoaderMock.Verify(x => x.LoadString(MainUrl), Times.Once);
            mocks.LoaderMock.Verify(x => x.LoadString("https://site1.com"), Times.Never,
                "https and http urls should be treated as same");
        }

        [Fact]
        public async Task ShouldMapSameUrlsToSameDirectoryOnAllDepths()
        {
            const string mainPath = "main";
            const string subPath = "sub";

            var mainPageContent = "<body>" +
                                  $"<a href=\"{SubUrl}\"> </a>" +
                                  $"<a href=\"{SubUrl}\"> </a></body>";
            var subPageContent = "<body>" +
                                 $"<a href=\"{SubUrl}\"> </a>" +
                                 $"<a href=\"{MainUrl}\"> </a>" +
                                 "</body>";

            var mocks = CreateMocksAndProcessor(new Configuration
            {
                RootLink = MainUrl,
                Depth = 1
            });

            mocks.UrlMapperMock.Setup(m => m.CreatePath(MainUrl, It.IsAny<NodeType?>()))
                .Returns(mainPath);
            mocks.UrlMapperMock.Setup(m => m.CreatePath(SubUrl, It.IsAny<NodeType?>()))
                .Returns(subPath);

            mocks.LoaderMock.Setup(x => x.LoadString(MainUrl))
                .ReturnsAsync(mainPageContent);
            mocks.LoaderMock.Setup(x => x.LoadString(SubUrl))
                .ReturnsAsync(subPageContent);

            await mocks.Processor.Run().ConfigureAwait(false);

            var modifiedMainPage = mainPageContent.Replace(SubUrl, subPath).Replace(MainUrl, mainPath);
            var modifiedSubPage = subPageContent.Replace(SubUrl, subPath).Replace(MainUrl, mainPath);

            mocks.WriterMock.Verify(m => m.Write(It.IsAny<Item>()), Times.Exactly(2));
            mocks.WriterMock.Verify(m => m.Write(It.Is<Item>(i => i.Content == modifiedMainPage && i.Uri == MainUrl)),
                Times.Once);
            mocks.WriterMock.Verify(m => m.Write(It.Is<Item>(i => i.Content == modifiedSubPage && i.Uri == SubUrl)),
                Times.Once);
        }

        [Fact]
        public async Task ShouldRemoveCrossOriginData()
        {
            // ReSharper disable once StringLiteralTypo - an example of hash in an element.
            const string crossOriginPart =
                "integrity=\"sha384-/Y6pD6FV/Vv2HJnA6t+vslU6fwYXjCFtcEpHbNJ0lyAFsXTsjBbfaDjzALeQsN6M\" " +
                "crossorigin=\"anonymous\"";
            var content = $"<body><link rel=\"stylesheet\" href=\"https://cdn.min.css\"{crossOriginPart}></body>";
            var clearedContent = content.Replace(crossOriginPart, string.Empty);

            var mocks = CreateMocksAndProcessor(new Configuration
            {
                RootLink = MainUrl,
                Depth = 1
            });

            mocks.LoaderMock.Setup(x => x.LoadString(MainUrl)).ReturnsAsync(content);

            await mocks.Processor.Run().ConfigureAwait(false);

            mocks.WriterMock.Verify(w => w.Write(It.IsAny<Item>()), Times.Once);
            mocks.WriterMock.Verify(w => w.Write(It.Is<Item>(i => i.Uri == MainUrl && i.Content == clearedContent)),
                Times.Once);
        }

        [Fact]
        public async Task ShouldTextContent()
        {
            const string styleUrl = "http://site1.com/css/style.css";
            const string mainContent = "<body><a href=\"css/style.css\"> </a></body>";
            const string styleContent = "some text";

            var mocks = CreateMocksAndProcessor(new Configuration
            {
                RootLink = MainUrl,
                Depth = 1
            });

            mocks.LoaderMock.Setup(x => x.LoadString(MainUrl))
                .ReturnsAsync(mainContent);
            mocks.LoaderMock.Setup(x => x.LoadString(styleUrl))
                .ReturnsAsync(styleContent);

            await mocks.Processor.Run().ConfigureAwait(false);

            mocks.WriterMock.Verify(m => m.Write(It.Is<Item>(i => i.Uri == MainUrl && i.Content == mainContent)),
                Times.Once);
            mocks.WriterMock.Verify(m => m.Write(It.Is<Item>(i => i.Uri == styleUrl && i.Content == styleContent)),
                Times.Once);
        }

        [Fact]
        public async Task ShouldThrowsIfCancellationRequested()
        {
            using var cts = new CancellationTokenSource();
            var mocks = CreateMocksAndProcessor(new Configuration
            {
                RootLink = MainUrl,
                Depth = 1
            }, cts.Token);

            mocks.LoaderMock.Setup(x => x.LoadString(MainUrl))
                .ReturnsAsync($"<body><a href=\"{SubUrl}\"> </a></body>");
            mocks.LoaderMock.Setup(x => x.LoadString(SubUrl))
                .ReturnsAsync("");

            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(
                    async () => await mocks.Processor.Run().ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task ShouldThrowsIfRootLinkIsInvalid()
        {
            var mocks = CreateMocksAndProcessor(new Configuration
            {
                RootLink = "//",
                Depth = 1
            });

            await Assert.ThrowsAsync<InvalidOperationException>(
                    async () => await mocks.Processor.Run().ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task ShouldThrowsIfUnhandledExceptionThrown()
        {
            var mocks = CreateMocksAndProcessor(new Configuration
            {
                RootLink = "//",
                Depth = 1
            });

            mocks.LoaderMock.Setup(x => x.LoadString(MainUrl))
                .ReturnsAsync($"<body><a href=\"{SubUrl}\"> </a></body>");
            mocks.LoaderMock.Setup(x => x.LoadString(SubUrl))
                .ThrowsAsync(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(
                    async () => await mocks.Processor.Run().ConfigureAwait(false))
                .ConfigureAwait(false);
        }
    }
}