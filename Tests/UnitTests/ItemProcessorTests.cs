using System;
using System.Threading;
using System.Threading.Tasks;
using Crawler;
using Crawler.Logic;
using Crawler.Projections;
using Moq;
using Shouldly;
using Xunit;

namespace Tests.UnitTests
{
    public class ItemProcessorTests
    {
        public ItemProcessorTests()
        {
            _cfg = new Configuration(MainUrl, Path)
            {
                Depth = 1,
                Mode = TraversalMode.SameHost
            };
        }

        private const string MainUrl = "http://site1.com";
        private const string Path = "C:\\";
        private const string SubUrl = "http://site1.com/sub-page";
        private static readonly Uri MainUri = MainUrl.AsUri();
        private static readonly Uri SubUri = SubUrl.AsUri();

        private readonly Configuration _cfg;

        private ProcessorTestContainer CreateMocksAndProcessor(Configuration? cfg = null,
            CancellationToken? token = null)
        {
            cfg ??= _cfg;

            var fileLoaderMock = new Mock<IFileLoader>();
            var urlMapperMock = new Mock<IUriMapper>();
            var writerMock = new Mock<IItemWriter>();

            return new ProcessorTestContainer(fileLoaderMock, urlMapperMock, writerMock,
                new ItemProcessor(fileLoaderMock.Object, new ItemParser(urlMapperMock.Object, cfg.Mode),
                    writerMock.Object, cfg, token ?? CancellationToken.None));
        }

        private class ProcessorTestContainer
        {
            public ProcessorTestContainer(Mock<IFileLoader> loaderMock, Mock<IUriMapper> urlMapperMock,
                Mock<IItemWriter> writerMock, ItemProcessor processor)
            {
                LoaderMock = loaderMock;
                UrlMapperMock = urlMapperMock;
                WriterMock = writerMock;
                Processor = processor;
            }

            public Mock<IFileLoader> LoaderMock { get; }
            public Mock<IUriMapper> UrlMapperMock { get; }
            public Mock<IItemWriter> WriterMock { get; }
            public ItemProcessor Processor { get; }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldLoadUrlsFromDifferentDomainsDependingOnConfiguration(bool fullTraversal)
        {
            var subUrl = "http://site2.com".AsUri();
            var mocks = CreateMocksAndProcessor(new Configuration(MainUrl, Path)
            {
                Depth = 1,
                Mode = fullTraversal ? TraversalMode.AnyHost : TraversalMode.SameHost
            });

            mocks.LoaderMock.Setup(x => x.LoadStringAsync(MainUri))
                .ReturnsAsync($"<body><a href=\"{subUrl}\"> </a></body>");
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(subUrl))
                .ReturnsAsync("<body></body>");

            await mocks.Processor.RunAsync().ConfigureAwait(false);

            mocks.LoaderMock.Verify(l => l.LoadStringAsync(It.IsAny<Uri>()), Times.Exactly(fullTraversal ? 2 : 1));
            mocks.LoaderMock.Verify(l => l.LoadStringAsync(MainUri), Times.Once);
            mocks.LoaderMock.Verify(l => l.LoadStringAsync(subUrl), fullTraversal ? Times.Once() : Times.Never());
        }

        [Fact]
        public async Task ShouldIgnoreItemInCaseOfFailedLoadingOrWithUnsuccessfulResponseCode()
        {
            var mocks = CreateMocksAndProcessor();

            mocks.LoaderMock.Setup(x => x.LoadStringAsync(MainUri))
                .ReturnsAsync($"<body><a href=\"{SubUrl}\"> </a></body>");
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(SubUri))
                .ReturnsAsync((string?) null);

            await mocks.Processor.RunAsync().ConfigureAwait(false);

            mocks.LoaderMock.Verify(l => l.LoadStringAsync(MainUri), Times.Once);
            mocks.LoaderMock.Verify(l => l.LoadStringAsync(SubUri), Times.Once);
            mocks.WriterMock.Verify(w => w.WriteAsync(It.IsAny<Item>()), Times.Once);
            mocks.WriterMock.Verify(w => w.WriteAsync(It.Is<Item>(i => i.Uri == MainUri)), Times.Once);
        }

        [Fact]
        public async Task ShouldLoadStaticContent()
        {
            var styleNormalizedUrl = "http://site1.com/css/style.css".AsUri();
            const string styleUrl = "css/style.css";
            const string mainContent = "<body><a href=\"css/style.css\"></a></body>";
            const string styleContent = "some text";
            const string stylePath = "style.css";

            var mocks = CreateMocksAndProcessor();

            mocks.LoaderMock.Setup(x => x.LoadStringAsync(MainUri))
                .ReturnsAsync(mainContent);
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(styleNormalizedUrl))
                .ReturnsAsync(styleContent);

            mocks.UrlMapperMock.Setup(x => x.CreatePath(styleNormalizedUrl, It.IsAny<NodeType?>()))
                .Returns(stylePath);

            await mocks.Processor.RunAsync().ConfigureAwait(false);

            mocks.WriterMock.Verify(
                m => m.WriteAsync(
                    It.Is<Item>(i => i.Uri == MainUri && i.Content == mainContent.Replace(styleUrl, stylePath))),
                Times.Once);
            mocks.WriterMock.Verify(
                m => m.WriteAsync(It.Is<Item>(i => i.Uri == styleNormalizedUrl && i.Content == styleContent)),
                Times.Once);
        }

        [Fact]
        public async Task ShouldLoadSubUrls()
        {
            var mocks = CreateMocksAndProcessor();

            mocks.LoaderMock.Setup(x => x.LoadStringAsync(MainUri))
                .ReturnsAsync($"<body><a href=\"{SubUrl}\"> </a></body>");
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(SubUri))
                .ReturnsAsync("<body></body>");

            await mocks.Processor.RunAsync().ConfigureAwait(false);

            mocks.LoaderMock.Verify(m => m.LoadStringAsync(MainUri), Times.Once);
            mocks.LoaderMock.Verify(m => m.LoadStringAsync(SubUri), Times.Once);
            mocks.WriterMock.Verify(m => m.WriteAsync(It.IsAny<Item>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ShouldLoadTooDeepUrlsIfTheyAreAccessibleFromHigherLevels()
        {
            var deepUri = "http://site1.com/too-deep-page".AsUri();
            var mocks = CreateMocksAndProcessor();

            mocks.LoaderMock.Setup(x => x.LoadStringAsync(MainUri))
                .ReturnsAsync($"<body><a href=\"{SubUrl}\"> </a>" +
                              $"<a href=\"{deepUri.OriginalString}\"> </a></body>");
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(SubUri))
                .ReturnsAsync($"<body><a href=\"{deepUri.OriginalString}\"> </a></body>");
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(deepUri))
                .ReturnsAsync("<body></body>");

            await mocks.Processor.RunAsync().ConfigureAwait(false);

            mocks.LoaderMock.Verify(l => l.LoadStringAsync(It.IsAny<Uri>()), Times.Exactly(3));
            mocks.LoaderMock.Verify(l => l.LoadStringAsync(MainUri), Times.Once);
            mocks.LoaderMock.Verify(l => l.LoadStringAsync(SubUri), Times.Once);
            mocks.LoaderMock.Verify(l => l.LoadStringAsync(deepUri), Times.Once);
        }

        [Fact]
        public async Task ShouldLoadUrlContentOnlyOnce()
        {
            var mocks = CreateMocksAndProcessor();

            mocks.LoaderMock.Setup(x => x.LoadStringAsync(MainUri))
                .ReturnsAsync("<body>" +
                              $"<a href=\"{SubUrl}\"> </a>" +
                              $"<a href=\"{SubUrl}\"> </a></body>");
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(SubUri))
                .ReturnsAsync("<body>" +
                              $"<a href=\"{SubUrl}\"> </a>" +
                              $"<a href=\"{MainUrl}\"> </a>" +
                              "</body>");

            await mocks.Processor.RunAsync().ConfigureAwait(false);

            mocks.LoaderMock.Verify(x => x.LoadStringAsync(SubUri), Times.Once);
            mocks.LoaderMock.Verify(x => x.LoadStringAsync(MainUri), Times.Once);
            mocks.LoaderMock.Verify(x => x.LoadStringAsync("https://site1.com".AsUri()), Times.Never,
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

            var mocks = CreateMocksAndProcessor();

            mocks.UrlMapperMock.Setup(m => m.CreatePath(MainUri, It.IsAny<NodeType?>()))
                .Returns(mainPath);
            mocks.UrlMapperMock.Setup(m => m.CreatePath(SubUri, It.IsAny<NodeType?>()))
                .Returns(subPath);
            mocks.UrlMapperMock.Setup(m => m.GetPath(MainUri))
                .Returns(mainPath);
            mocks.UrlMapperMock.Setup(m => m.GetPath(SubUri))
                .Returns(subPath);

            mocks.LoaderMock.Setup(x => x.LoadStringAsync(MainUri))
                .ReturnsAsync(mainPageContent);
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(SubUri))
                .ReturnsAsync(subPageContent);

            await mocks.Processor.RunAsync().ConfigureAwait(false);

            var modifiedMainPage = mainPageContent.Replace(SubUrl, subPath).Replace(MainUrl, mainPath);
            var modifiedSubPage = subPageContent.Replace(SubUrl, subPath).Replace(MainUrl, mainPath);

            mocks.WriterMock.Verify(m => m.WriteAsync(It.IsAny<Item>()), Times.Exactly(2));
            mocks.WriterMock.Verify(
                m => m.WriteAsync(It.Is<Item>(i => i.Content == modifiedMainPage && i.Uri == MainUri)),
                Times.Once);
            mocks.WriterMock.Verify(
                m => m.WriteAsync(It.Is<Item>(i => i.Content == modifiedSubPage && i.Uri == SubUri)),
                Times.Once);
        }

        [Fact]
        public async Task ShouldMutateTooDeepLinksIfTheyAreAccessibleFromHigherLevels()
        {
            var deepUri = "http://site1.com/too-deep-page".AsUri();
            const string mainPath = "main";
            const string subPath = "sub";
            const string deepPath = "deep";
            const string deepContent = "<body></body>";

            var mocks = CreateMocksAndProcessor();

            var mainContent = $"<body><a href=\"{SubUrl}\"> </a><a href=\"{deepUri.OriginalString}\"> </a></body>";
            var subContent = $"<body><a href=\"{deepUri.OriginalString}\"> </a></body>";
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(MainUri))
                .ReturnsAsync(mainContent);
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(SubUri))
                .ReturnsAsync(subContent);
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(deepUri))
                .ReturnsAsync("<body></body>");

            mocks.UrlMapperMock.Setup(m => m.CreatePath(MainUri, It.IsAny<NodeType?>()))
                .Returns(mainPath);
            mocks.UrlMapperMock.Setup(m => m.CreatePath(SubUri, It.IsAny<NodeType?>()))
                .Returns(subPath);
            mocks.UrlMapperMock.Setup(m => m.CreatePath(deepUri, It.IsAny<NodeType?>()))
                .Returns(deepPath);
            mocks.UrlMapperMock.Setup(m => m.GetPath(deepUri))
                .Returns(deepPath);

            await mocks.Processor.RunAsync().ConfigureAwait(false);

            mocks.WriterMock.Verify(l => l.WriteAsync(It.IsAny<Item>()), Times.Exactly(3));
            mocks.WriterMock.Verify(
                l => l.WriteAsync(It.Is<Item>(i =>
                    i.Content == mainContent.Replace(SubUrl, subPath).Replace(deepUri.OriginalString, deepPath))),
                Times.Once);
            mocks.WriterMock.Verify(
                l => l.WriteAsync(It.Is<Item>(i => i.Content == subContent.Replace(deepUri.OriginalString, deepPath))),
                Times.Once);
            mocks.WriterMock.Verify(l => l.WriteAsync(It.Is<Item>(i => i.Content == deepContent)), Times.Once);
        }

        [Fact]
        public async Task ShouldNotChangeContentInSnapshotMode()
        {
            var mocks = CreateMocksAndProcessor(new Configuration(MainUrl, Path)
            {
                Depth = 1,
                Mode = TraversalMode.SameHostSnapshot
            });

            var mainContent = $"<body><a href=\"{SubUrl}\"></a></body>";
            var subContent = "<body></body>";

            mocks.LoaderMock.Setup(x => x.LoadStringAsync(MainUri))
                .ReturnsAsync(mainContent);
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(SubUri))
                .ReturnsAsync(subContent);

            await mocks.Processor.RunAsync().ConfigureAwait(false);

            mocks.WriterMock.Verify(m => m.WriteAsync(It.IsAny<Item>()), Times.Exactly(2));
            mocks.WriterMock.Verify(m => m.WriteAsync(It.Is<Item>(i => i.Content == mainContent && i.Uri == MainUri)),
                Times.Exactly(1));
            mocks.WriterMock.Verify(m => m.WriteAsync(It.Is<Item>(i => i.Content == subContent && i.Uri == SubUri)),
                Times.Exactly(1));
        }

        [Fact]
        public async Task ShouldNotLoadTooDeepUrls()
        {
            var deepUri = "http://site1.com/too-deep-page".AsUri();
            var mocks = CreateMocksAndProcessor();

            mocks.LoaderMock.Setup(x => x.LoadStringAsync(MainUri))
                .ReturnsAsync($"<body><a href=\"{SubUrl}\"> </a></body>");
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(SubUri))
                .ReturnsAsync($"<body><a href=\"{deepUri.OriginalString}\"> </a></body>");
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(deepUri))
                .ReturnsAsync("<body></body>");

            await mocks.Processor.RunAsync().ConfigureAwait(false);

            mocks.LoaderMock.Verify(l => l.LoadStringAsync(It.IsAny<Uri>()), Times.Exactly(2));
            mocks.LoaderMock.Verify(l => l.LoadStringAsync(MainUri), Times.Once);
            mocks.LoaderMock.Verify(l => l.LoadStringAsync(SubUri), Times.Once);
            mocks.LoaderMock.Verify(l => l.LoadStringAsync(deepUri), Times.Never);
        }

        [Fact]
        public async Task ShouldNotMutateTooDeepLinks()
        {
            var deepUri = "http://site1.com/too-deep-page".AsUri();
            const string mainPath = "main";
            const string subPath = "sub";
            const string deepPath = "deep";

            var mocks = CreateMocksAndProcessor();

            var mainContent = $"<body><a href=\"{SubUrl}\"> </a></body>";
            var subContent = $"<body><a href=\"{deepUri.OriginalString}\"> </a></body>";
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(MainUri))
                .ReturnsAsync(mainContent);
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(SubUri))
                .ReturnsAsync(subContent);

            mocks.UrlMapperMock.Setup(m => m.CreatePath(MainUri, It.IsAny<NodeType?>()))
                .Returns(mainPath);
            mocks.UrlMapperMock.Setup(m => m.CreatePath(SubUri, It.IsAny<NodeType?>()))
                .Returns(subPath);
            mocks.UrlMapperMock.Setup(m => m.CreatePath(deepUri, It.IsAny<NodeType?>()))
                .Returns(deepPath);

            await mocks.Processor.RunAsync().ConfigureAwait(false);

            mocks.WriterMock.Verify(l => l.WriteAsync(It.IsAny<Item>()), Times.Exactly(2));
            mocks.WriterMock.Verify(
                l => l.WriteAsync(It.Is<Item>(i => i.Content == mainContent.Replace(SubUrl, subPath))),
                Times.Once);
            mocks.WriterMock.Verify(
                l => l.WriteAsync(It.Is<Item>(i => i.Content == subContent.Replace(deepUri.OriginalString, deepPath))),
                Times.Never);
            mocks.WriterMock.Verify(l => l.WriteAsync(It.Is<Item>(i => i.Content == subContent)), Times.Once);
        }

        [Fact]
        public async Task ShouldNotProcessMailNodes()
        {
            var mocks = CreateMocksAndProcessor(new Configuration(MainUrl, Path)
            {
                Depth = 1,
                Mode = TraversalMode.AnyHost
            });

            const string content = "<body><a href=\"mailto:uuu@domain.com\"> </a></body>";
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(MainUri))
                .ReturnsAsync(content);

            await mocks.Processor.RunAsync().ConfigureAwait(false);

            mocks.WriterMock.Verify(l => l.WriteAsync(It.Is<Item>(i => i.Content == content)), Times.Once);
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

            var mocks = CreateMocksAndProcessor();

            mocks.LoaderMock.Setup(x => x.LoadStringAsync(MainUri)).ReturnsAsync(content);

            await mocks.Processor.RunAsync().ConfigureAwait(false);

            mocks.WriterMock.Verify(w => w.WriteAsync(It.IsAny<Item>()), Times.Once);
            mocks.WriterMock.Verify(
                w => w.WriteAsync(It.Is<Item>(i => i.Uri == MainUri && i.Content == clearedContent)),
                Times.Once);
        }

        [Fact]
        public async Task ShouldThrowsIfCancellationRequested()
        {
            using var cts = new CancellationTokenSource();
            var mocks = CreateMocksAndProcessor(null, cts.Token);

            mocks.LoaderMock.Setup(x => x.LoadStringAsync(MainUri))
                .ReturnsAsync($"<body><a href=\"{SubUrl}\"> </a></body>");
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(SubUri))
                .ReturnsAsync("");

            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(
                    async () => await mocks.Processor.RunAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task ShouldThrowsIfRootLinkIsInvalid()
        {
            var mocks = CreateMocksAndProcessor(new Configuration("//", Path)
            {
                Depth = 1
            });

            await Should.ThrowAsync<InvalidOperationException>(
                    async () => await mocks.Processor.RunAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task ShouldThrowsIfUnhandledExceptionThrown()
        {
            var mocks = CreateMocksAndProcessor(new Configuration("//", Path)
            {
                Depth = 1
            });

            mocks.LoaderMock.Setup(x => x.LoadStringAsync(MainUri))
                .ReturnsAsync($"<body><a href=\"{SubUrl}\"> </a></body>");
            mocks.LoaderMock.Setup(x => x.LoadStringAsync(SubUri))
                .ThrowsAsync(new InvalidOperationException());

            await Should.ThrowAsync<InvalidOperationException>(
                    async () => await mocks.Processor.RunAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
        }
    }
}