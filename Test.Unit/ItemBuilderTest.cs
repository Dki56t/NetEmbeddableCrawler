using System.Linq;
using System.Net;
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
        private UrlMapper _mapper;

        [TestInitialize]
        public void Init()
        {
            _cfg = new Configuration
            {
                RootLink = "http://site1.com",
                Depth = 3,
                FullTraversal = false
            };
            _mapper = new Mock<UrlMapper>(_cfg).Object;
        }

        [TestMethod]
        public void TestItemTreeBuilding()
        {
            var loader = new Mock<FileLoader>();
            loader.Setup(x => x.LoadString("http://site1.com")).Returns(Task.FromResult("<body><a href=\"http://site1.com/sub-page\"> </a></body>"));
            loader.Setup(x => x.LoadString("http://site1.com/sub-page")).Returns(Task.FromResult("<body></body>"));

            var builder = new ItemBuilder(_cfg, _mapper);
            var item = builder.Build(loader.Object).Result;

            Assert.AreEqual(item.Path, "http://site1.com");
            Assert.AreEqual(item.GetSubItems().Count, 1);
            Assert.AreEqual(item.GetSubItems().Single().Path, "http://site1.com/sub-page");
        }

        [TestMethod]
        public void TestLoadingHappensOnlyOnce()
        {
            var loader = new Mock<FileLoader>();
            loader.Setup(x => x.LoadString("http://site1.com")).Returns(Task.FromResult("<body>" +
                                                                        "<a href=\"http://site1.com/sub-page\"> </a>" +
                                                                        "<a href=\"http://site1.com/sub-page\"> </a>" +
                                                                        "<a href=\"http://site1.com/sub-page\"> </a></body>"));
            loader.Setup(x => x.LoadString("http://site1.com/sub-page")).Returns(Task.FromResult("<body>" +
                                                                                 "<a href=\"http://site1.com/sub-page\"> </a>" +
                                                                                 "<a href=\"http://site1.com/\"> </a>" +
                                                                                 "</body>"));

            var builder = new ItemBuilder(_cfg, _mapper);
            var item = builder.Build(loader.Object).Result;

            Assert.AreEqual(item.Path, "http://site1.com");
            Assert.AreEqual(item.GetSubItems().Count, 1);
            Assert.AreEqual(item.GetSubItems().Single().Path, "http://site1.com/sub-page");
            loader.Verify(x => x.LoadString("http://site1.com/sub-page"), Times.Once);
            loader.Verify(x => x.LoadString("http://site1.com"), Times.Once);
        }
    }
}
