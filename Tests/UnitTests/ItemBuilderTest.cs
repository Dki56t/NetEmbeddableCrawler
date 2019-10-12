using System.Linq;
using System.Threading.Tasks;
using Crawler;
using Crawler.Logic;
using Moq;
using Xunit;

namespace Tests.UnitTests
{
    public class ItemBuilderTest
    {
        public ItemBuilderTest()
        {
            _cfg = new Configuration
            {
                RootLink = "http://site1.com",
                Depth = 3,
                FullTraversal = false
            };
        }

        private readonly Configuration _cfg;

        [Fact]
        public void TestItemTreeBuilding()
        {
            var mapper = new Mock<IUrlMapper>().Object;
            var loader = new Mock<IFileLoader>();
            loader.Setup(x => x.LoadString("http://site1.com"))
                .Returns(Task.FromResult("<body><a href=\"http://site1.com/sub-page\"> </a></body>"));
            loader.Setup(x => x.LoadString("http://site1.com/sub-page")).Returns(Task.FromResult("<body></body>"));

            var builder = new ItemBuilder(_cfg, mapper);
            var item = builder.Build(loader.Object).Result;

            Assert.Equal(item.Uri, "http://site1.com");
            Assert.Equal(item.GetSubItems().Count, 1);
            Assert.Equal(item.GetSubItems().Single().Uri, "http://site1.com/sub-page");
        }

        [Fact]
        public void TestLoadingHappensOnlyOnce()
        {
            var mapper = new Mock<IUrlMapper>().Object;
            var loader = new Mock<IFileLoader>();
            loader.Setup(x => x.LoadString("http://site1.com")).Returns(Task.FromResult("<body>" +
                                                                                        "<a href=\"http://site1.com/sub-page\"> </a>" +
                                                                                        "<a href=\"http://site1.com/sub-page\"> </a>" +
                                                                                        "<a href=\"http://site1.com/sub-page\"> </a></body>"));
            loader.Setup(x => x.LoadString("http://site1.com/sub-page")).Returns(Task.FromResult("<body>" +
                                                                                                 "<a href=\"http://site1.com/sub-page\"> </a>" +
                                                                                                 "<a href=\"http://site1.com/\"> </a>" +
                                                                                                 "</body>"));

            var builder = new ItemBuilder(_cfg, mapper);
            var item = builder.Build(loader.Object).Result;

            Assert.Equal(item.Uri, "http://site1.com");
            Assert.Equal(item.GetSubItems().Count, 1);
            Assert.Equal(item.GetSubItems().Single().Uri, "http://site1.com/sub-page");
            loader.Verify(x => x.LoadString("http://site1.com/sub-page"), Times.Once);
            loader.Verify(x => x.LoadString("http://site1.com"), Times.Once);
        }

        [Fact]
        public void TestRemoveCrossOriginUri()
        {
            var mapper = new Mock<IUrlMapper>();
            var loader = new Mock<IFileLoader>();
            loader.Setup(x => x.LoadString("http://site1.com")).Returns(Task.FromResult(
                "<body><link rel=\"stylesheet\" href=\"https://cdn.min.css\" integrity=\"sha384-/Y6pD6FV/Vv2HJnA6t+vslU6fwYXjCFtcEpHbNJ0lyAFsXTsjBbfaDjzALeQsN6M\" crossorigin=\"anonymous\"></body>"));

            var builder = new ItemBuilder(_cfg, mapper.Object);
            var item = builder.Build(loader.Object).Result;

            Assert.Equal(item.Content, "<body><link rel=\"stylesheet\" href=\"https://cdn.min.css\"></body>");
        }

        [Fact]
        public void TestStylesKeepsAsLocalUri()
        {
            var mapper = new Mock<IUrlMapper>();
            var loader = new Mock<IFileLoader>();
            loader.Setup(x => x.LoadString("http://site1.com"))
                .Returns(Task.FromResult("<body><a href=\"css/style.css\"> </a></body>"));
            loader.Setup(x => x.LoadString("http://site1.com/css/style.css")).Returns(Task.FromResult(""));
            mapper.Setup(x => x.GetPath("css/style.css", NodeType.Text))
                .Returns("directory/css/style.css");

            var builder = new ItemBuilder(_cfg, mapper.Object);
            var item = builder.Build(loader.Object).Result;

            Assert.Equal(item.Content, "<body><a href=\"css/style.css\"> </a></body>");
        }
    }
}