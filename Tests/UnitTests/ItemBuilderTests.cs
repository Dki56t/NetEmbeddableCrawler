using System;
using System.Linq;
using System.Threading.Tasks;
using Crawler;
using Crawler.Logic;
using Moq;
using Xunit;

namespace Tests.UnitTests
{
    public class ItemBuilderTests
    {
        public ItemBuilderTests()
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

        [Fact]
        public async Task ShouldBuildTreeOfItems()
        {
            var loaderMock = new Mock<IFileLoader>();
            loaderMock.Setup(x => x.LoadString("http://site1.com"))
                .ReturnsAsync("<body><a href=\"http://site1.com/sub-page\"> </a></body>");
            loaderMock.Setup(x => x.LoadString("http://site1.com/sub-page"))
                .ReturnsAsync("<body></body>");

            var builder = new ItemBuilder(_cfg, new Mock<IUrlMapper>().Object, loaderMock.Object);

            var item = await builder.Build().ConfigureAwait(false);

            Assert.Equal("http://site1.com", item.Uri);
            Assert.Equal(1, item.GetSubItems().Count);
            Assert.Equal("http://site1.com/sub-page", item.GetSubItems().Single().Uri);
        }

        [Fact]
        public async Task ShouldChangeUrlsToStaticContent()
        {
            var mapperMock = new Mock<IUrlMapper>();
            mapperMock.Setup(x => x.GetPath("css/style.css", NodeType.Text))
                .Returns("directory/css/style.css");

            var loaderMock = new Mock<IFileLoader>();
            loaderMock.Setup(x => x.LoadString("http://site1.com"))
                .ReturnsAsync("<body><a href=\"css/style.css\"> </a></body>");
            loaderMock.Setup(x => x.LoadString("http://site1.com/css/style.css"))
                .ReturnsAsync("");

            var builder = new ItemBuilder(_cfg, mapperMock.Object, loaderMock.Object);

            var item = await builder.Build().ConfigureAwait(false);

            Assert.Equal("<body><a href=\"css/style.css\"> </a></body>", item.Content);
        }

        [Fact]
        public async Task ShouldLoadUrlContentOnlyOnce()
        {
            var loaderMock = new Mock<IFileLoader>();
            loaderMock.Setup(x => x.LoadString("http://site1.com"))
                .ReturnsAsync("<body>" +
                              "<a href=\"http://site1.com/sub-page\"> </a>" +
                              "<a href=\"http://site1.com/sub-page\"> </a>" +
                              "<a href=\"http://site1.com/sub-page\"> </a></body>");
            loaderMock.Setup(x => x.LoadString("http://site1.com/sub-page"))
                .ReturnsAsync("<body>" +
                              "<a href=\"http://site1.com/sub-page\"> </a>" +
                              "<a href=\"https://site1.com/\"> </a>" +
                              "</body>");

            var builder = new ItemBuilder(_cfg, new Mock<IUrlMapper>().Object, loaderMock.Object);

            var item = await builder.Build().ConfigureAwait(false);

            Assert.Equal("http://site1.com", item.Uri);
            Assert.Equal(1, item.GetSubItems().Count);
            Assert.Equal("http://site1.com/sub-page", item.GetSubItems().Single().Uri);
            loaderMock.Verify(x => x.LoadString("http://site1.com/sub-page"), Times.Once);
            loaderMock.Verify(x => x.LoadString("http://site1.com"), Times.Once);
            loaderMock.Verify(x => x.LoadString("https://site1.com"), Times.Never,
                "https and http urls should be treated as same");
        }

        [Fact]
        public async Task ShouldMapSameUrlsToSameDirectoryOnAllDepths()
        {
            const string mainPath = "main";
            const string subPath = "sub";

            var mainPageContent = "<body>" +
                                  $"<a href=\"{SubUrl}\"> </a>" +
                                  $"<a href=\"{SubUrl}\"> </a>" +
                                  $"<a href=\"{SubUrl}\"> </a></body>";
            var subPageContent = "<body>" +
                                 $"<a href=\"{SubUrl}\"> </a>" +
                                 $"<a href=\"{MainUrl}\"> </a>" +
                                 "</body>";

            var mapperMock = new Mock<IUrlMapper>();
            mapperMock.Setup(m => m.GetPath(MainUrl, It.IsAny<NodeType?>()))
                .Returns(mainPath);
            mapperMock.Setup(m => m.GetPath(SubUrl, It.IsAny<NodeType?>()))
                .Returns(subPath);

            var loaderMock = new Mock<IFileLoader>();
            loaderMock.Setup(x => x.LoadString(MainUrl))
                .ReturnsAsync(mainPageContent);
            loaderMock.Setup(x => x.LoadString(SubUrl))
                .ReturnsAsync(subPageContent);

            var builder = new ItemBuilder(_cfg, mapperMock.Object, loaderMock.Object);

            var item = await builder.Build().ConfigureAwait(false);

            var modifiedMainPage = mainPageContent.Replace(SubUrl, subPath).Replace(MainUrl, mainPath);
            var modifiedSubPage = subPageContent.Replace(SubUrl, subPath).Replace(MainUrl, mainPath);

            Assert.Equal(modifiedMainPage, item.Content);

            var subItems = item.GetSubItems();
            Assert.Equal(1, subItems.Count);
            Assert.Equal(modifiedSubPage, subItems.First().Content);
        }

        [Fact]
        public async Task ShouldNotAttachItemInCaseOfFailedLoadingOrWithUnsuccessfulResponseCode()
        {
            var mapperMock = new Mock<IUrlMapper>();
            var loaderMock = new Mock<IFileLoader>();
            loaderMock.Setup(x => x.LoadString(MainUrl))
                .ReturnsAsync($"<body><a href=\"{SubUrl}\"> </a></body>");
            loaderMock.Setup(x => x.LoadString(SubUrl))
                .ReturnsAsync((string) null);

            var builder = new ItemBuilder(new Configuration
            {
                Depth = 3,
                RootLink = MainUrl
            }, mapperMock.Object, loaderMock.Object);

            var item = await builder.Build().ConfigureAwait(false);

            loaderMock.Verify(l => l.LoadString(MainUrl), Times.Once);
            loaderMock.Verify(l => l.LoadString(SubUrl), Times.Once);
            Assert.Equal(0, item.GetSubItems().Count);
        }

        [Fact]
        public async Task ShouldRemoveCrossOriginData()
        {
            var loaderMock = new Mock<IFileLoader>();
            loaderMock.Setup(x => x.LoadString("http://site1.com")).ReturnsAsync(
                // ReSharper disable once StringLiteralTypo - an example of hash in an element.
                "<body><link rel=\"stylesheet\" href=\"https://cdn.min.css\" " +
                "integrity=\"sha384-/Y6pD6FV/Vv2HJnA6t+vslU6fwYXjCFtcEpHbNJ0lyAFsXTsjBbfaDjzALeQsN6M\" " +
                "crossorigin=\"anonymous\"></body>");

            var mapperMock = new Mock<IUrlMapper>();

            var builder = new ItemBuilder(_cfg, mapperMock.Object, loaderMock.Object);

            var item = await builder.Build().ConfigureAwait(false);

            Assert.Equal("<body><link rel=\"stylesheet\" href=\"https://cdn.min.css\"></body>", item.Content);
        }

        [Fact]
        public async Task ShouldThrowIfRootLinkIsInvalid()
        {
            var mapperMock = new Mock<IUrlMapper>();
            var loaderMock = new Mock<IFileLoader>();
            var builder = new ItemBuilder(new Configuration
            {
                RootLink = "//"
            }, mapperMock.Object, loaderMock.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(
                    async () => await builder.Build().ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task ShouldThrowIfUnhandledExceptionThrown()
        {
            var loaderMock = new Mock<IFileLoader>();
            loaderMock.Setup(x => x.LoadString(MainUrl))
                .ReturnsAsync($"<body><a href=\"{SubUrl}\"> </a></body>");
            loaderMock.Setup(x => x.LoadString(SubUrl))
                .ThrowsAsync(new InvalidOperationException());

            var builder = new ItemBuilder(new Configuration
            {
                Depth = 3,
                RootLink = MainUrl
            }, new Mock<IUrlMapper>().Object, loaderMock.Object);

            await Assert.ThrowsAsync<AggregateException>(
                    async () => await builder.Build().ConfigureAwait(false))
                .ConfigureAwait(false);
        }
    }
}