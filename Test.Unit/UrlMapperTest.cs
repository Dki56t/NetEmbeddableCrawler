using System.IO;
using Crawler;
using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Unit
{
    [TestClass]
    public class UrlMapperTest
    {
        private string _testDirectoryPath;

        [TestInitialize]
        public void Init()
        {
            _testDirectoryPath = Path.Combine(Path.GetTempPath(), "TestFileWrite");
        }

        [TestMethod]
        public void TestMapping()
        {
            var item1 = new Item("1", "http://site1/index.html");
            var item11 = new Item("1/internal", "http://site1/internal/internal.html");
            item1.AddItem(item11);
            var item2 = new Item("2", "http://site2/index2.html");
            var item21 = new Item("2/internal", "http://site2/otherinternal/some.html");
            item2.AddItem(item21);

            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });

            Assert.AreEqual(mapper.GetPath(item1), Path.Combine(_testDirectoryPath, "site1\\index.html"));
            Assert.AreEqual(mapper.GetPath(item11), Path.Combine(_testDirectoryPath, "site1\\internal\\internal.html"));
            Assert.AreEqual(mapper.GetPath(item2), Path.Combine(_testDirectoryPath, "site2\\index2.html"));
            Assert.AreEqual(mapper.GetPath(item21),
                Path.Combine(_testDirectoryPath, "site2\\otherinternal\\some.html"));
        }

        [TestMethod]
        public void TestMappingPartial()
        {
            var item4 = new Item("4", "http://site4/#");
            var item41 = new Item("4/internal", "http://site4/test/some.html/#part-2");
            item4.AddItem(item41);

            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });

            Assert.AreEqual(mapper.GetPath(item4), Path.Combine(_testDirectoryPath, "site4\\index.html"));
            Assert.AreEqual(mapper.GetPath(item41), Path.Combine(_testDirectoryPath, "site4\\test\\some.html"));
        }

        [TestMethod]
        public void TestMappingAbnormal()
        {
            var item3 = new Item("3", "http://site3/");
            var item31 = new Item("3/internal", "http://site3/test/some.html/");
            item3.AddItem(item31);

            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });

            Assert.AreEqual(mapper.GetPath(item3), Path.Combine(_testDirectoryPath, "site3\\index.html"));
            Assert.AreEqual(mapper.GetPath(item31), Path.Combine(_testDirectoryPath, "site3\\test\\some.html"));
        }

        [TestMethod]
        public void TestFileNameAppendExtension()
        {
            var item1 = new Item("1", "http://site1/");
            var item11 = new Item("1/internal", "http://site1/test/some");
            item1.AddItem(item11);

            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });

            Assert.AreEqual(mapper.GetPath(item1), Path.Combine(_testDirectoryPath, "site1\\index.html"));
            Assert.AreEqual(mapper.GetPath(item11), Path.Combine(_testDirectoryPath, "site1\\test\\some.html"));
        }

        [TestMethod]
        public void TestUrlWithParameters()
        {
            var item1 = new Item("1", "http://site1/");
            var item11 = new Item("1/issues", "http://site1/issues?q=is%3Aopen+is%3Aissue");
            var item12 = new Item("1/issues", "http://site1/issues?q=is%3Aissue+is%3Aclosed");
            var item13 = new Item("1/issues", "http://site1/issues?q=is%3Aissue+is%3Aopen+sort%3Acreated-asc");
            var item14 = new Item("1/issues", "http://site1/issues?q=is%3Aissue+is%3Aopen+sort%3Acomments-desc");
            var item15 = new Item("1/issues", "http://site1/issues");
            item1.AddItem(item11);
            item1.AddItem(item12);
            item1.AddItem(item13);
            item1.AddItem(item14);
            item1.AddItem(item15);

            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });

            Assert.AreEqual(mapper.GetPath(item1), Path.Combine(_testDirectoryPath, "site1\\index.html"));
            Assert.AreEqual(mapper.GetPath(item11), Path.Combine(_testDirectoryPath, "site1\\issues_p_q=is%3Aopen+is%3Aissue.html"));
            Assert.AreEqual(mapper.GetPath(item12), Path.Combine(_testDirectoryPath, "site1\\issues_p_q=is%3Aissue+is%3Aclosed.html"));
            Assert.AreEqual(mapper.GetPath(item13), Path.Combine(_testDirectoryPath, "site1\\issues_p_q=is%3Aissue+is%3Aopen+sort%3Acreated-asc.html"));
            Assert.AreEqual(mapper.GetPath(item14), Path.Combine(_testDirectoryPath, "site1\\issues_p_q=is%3Aissue+is%3Aopen+sort%3Acomments-desc.html"));
            Assert.AreEqual(mapper.GetPath(item15), Path.Combine(_testDirectoryPath, "site1\\issues.html"));
        }

        [TestMethod]
        public void TestHostNameRecognition()
        {
            var item1 = new Item("1", "http://site1/test/some");

            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });
            mapper.GetPath(item1);

            Assert.AreEqual(mapper.GetProcessedPathByUrl("http://site1/test/some"), Path.Combine(_testDirectoryPath, "site1\\test\\some.html"));
            Assert.AreEqual(mapper.GetProcessedPathByUrl("http://site1/"), Path.Combine(_testDirectoryPath, "site1\\index.html"));
        }
    }
}
