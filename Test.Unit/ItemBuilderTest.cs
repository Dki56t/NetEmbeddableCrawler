using System.Linq;
using System.Threading.Tasks;
using Crawler;
using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Test.Unit
{
    [TestClass]
    public class ItemBuilderTest
    {
        private Configuration _cfg;

        [TestInitialize]
        public void Init()
        {
            _cfg = new Configuration
            {
                RootLink = "http://site1.com",
                Depth = 3,
                FullTraversal = false
            };
        }

        [TestMethod]
        public void TestItemTreeBuilding()
        {
            var mapper = new Mock<UrlMapper>(_cfg).Object;
            var loader = new Mock<FileLoader>();
            loader.Setup(x => x.LoadString("http://site1.com")).Returns(Task.FromResult("<body><a href=\"http://site1.com/sub-page\"> </a></body>"));
            loader.Setup(x => x.LoadString("http://site1.com/sub-page")).Returns(Task.FromResult("<body></body>"));

            var builder = new ItemBuilder(_cfg, mapper);
            var item = builder.Build(loader.Object).Result;

            Assert.AreEqual(item.Uri, "http://site1.com");
            Assert.AreEqual(item.GetSubItems().Count, 1);
            Assert.AreEqual(item.GetSubItems().Single().Uri, "http://site1.com/sub-page");
        }

        [TestMethod]
        public void TestLoadingHappensOnlyOnce()
        {
            var mapper = new Mock<UrlMapper>(_cfg).Object;
            var loader = new Mock<FileLoader>();
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

            Assert.AreEqual(item.Uri, "http://site1.com");
            Assert.AreEqual(item.GetSubItems().Count, 1);
            Assert.AreEqual(item.GetSubItems().Single().Uri, "http://site1.com/sub-page");
            loader.Verify(x => x.LoadString("http://site1.com/sub-page"), Times.Once);
            loader.Verify(x => x.LoadString("http://site1.com"), Times.Once);
        }

        [TestMethod]
        public void TestStylesKeepsAsLocalUri()
        {
            var mapper = new Mock<UrlMapper>(_cfg);
            var loader = new Mock<FileLoader>();
            loader.Setup(x => x.LoadString("http://site1.com")).Returns(Task.FromResult("<body><a href=\"css/style.css\"> </a></body>"));
            loader.Setup(x => x.LoadString("http://site1.com/css/style.css")).Returns(Task.FromResult(""));
            mapper.Setup(x => x.GetPath(It.Is<Item>(i => i.Uri == "css/style.css")))
                .Returns("directory/css/style.css");

            var builder = new ItemBuilder(_cfg, mapper.Object);
            var item = builder.Build(loader.Object).Result;

            Assert.AreEqual(item.Content, "<body><a href=\"css/style.css\"> </a></body>");
        }

        [TestMethod]
        public void TestRemoveCrossOriginUri()
        {
            var mapper = new Mock<UrlMapper>(_cfg);
            var loader = new Mock<FileLoader>();
            loader.Setup(x => x.LoadString("http://site1.com")).Returns(Task.FromResult("<body><link rel=\"stylesheet\" href=\"https://cdn.min.css\" integrity=\"sha384-/Y6pD6FV/Vv2HJnA6t+vslU6fwYXjCFtcEpHbNJ0lyAFsXTsjBbfaDjzALeQsN6M\" crossorigin=\"anonymous\"></body>"));

            var builder = new ItemBuilder(_cfg, mapper.Object);
            var item = builder.Build(loader.Object).Result;

            Assert.AreEqual(item.Content, "<body><link rel=\"stylesheet\" href=\"https://cdn.min.css\"></body>");
        }
    }
}
