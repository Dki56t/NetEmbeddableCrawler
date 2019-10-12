using System.Collections.Generic;
using System.IO;
using Crawler;
using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Test.Integration
{
    [TestClass]
    public class ItemWriterTest
    {
        private string _testDirectoryPath;

        [TestInitialize]
        public void Init()
        {
            _testDirectoryPath = Path.Combine(Path.GetTempPath(), "TestFileWrite");
            var di = new DirectoryInfo(_testDirectoryPath);
            if (di.Exists)
                di.Delete(true);
            else
                di.Create();
        }

        [TestMethod]
        public void TestFileWrite()
        {
            //setup
            var item1 = new Item("1", "http://site1/index.html");
            var item11 = new Item("1/internal", "http://site1/internal/internal.html");
            item1.AddItem(item11);
            var item2 = new Item("2", "http://site2/index.html");
            var item21 = new Item("2/internal", "http://site2/otherinternal/some.html");
            item2.AddItem(item21);
            item1.AddItem(item2);

            var mapper = new Mock<UrlMapper>(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });
            mapper.Setup(x => x.GetPath(item1.Uri, null))
                .Returns(Path.Combine(_testDirectoryPath, "site1\\index.html"));
            mapper.Setup(x => x.GetPath(item11.Uri, null))
                .Returns(Path.Combine(_testDirectoryPath, "site1\\internal\\internal.html"));
            mapper.Setup(x => x.GetPath(item2.Uri, null))
                .Returns(Path.Combine(_testDirectoryPath, "site2\\index.html"));
            mapper.Setup(x => x.GetPath(item21.Uri, null))
                .Returns(Path.Combine(_testDirectoryPath, "site2\\otherinternal\\some.html"));

            //act
            ItemWriter.Write(item1, mapper.Object);
            var fileNames = new List<string>();
            FillAllFileNames(new DirectoryInfo(_testDirectoryPath), fileNames);

            //verify
            Assert.AreEqual(fileNames.Count, 9);
            Assert.IsTrue(fileNames.Contains(_testDirectoryPath));
            Assert.IsTrue(fileNames.Contains(Path.Combine(_testDirectoryPath, "site1")));
            Assert.IsTrue(fileNames.Contains(Path.Combine(_testDirectoryPath, "site1\\index.html")));
            Assert.IsTrue(fileNames.Contains(Path.Combine(_testDirectoryPath, "site1\\internal")));
            Assert.IsTrue(fileNames.Contains(Path.Combine(_testDirectoryPath, "site1\\internal\\internal.html")));
            Assert.IsTrue(fileNames.Contains(Path.Combine(_testDirectoryPath, "site2")));
            Assert.IsTrue(fileNames.Contains(Path.Combine(_testDirectoryPath, "site2\\index.html")));
            Assert.IsTrue(fileNames.Contains(Path.Combine(_testDirectoryPath, "site2\\otherinternal")));
            Assert.IsTrue(fileNames.Contains(Path.Combine(_testDirectoryPath, "site2\\otherinternal\\some.html")));
        }

        private static void FillAllFileNames(DirectoryInfo di, List<string> fileNames)
        {
            fileNames.Add(di.FullName);
            foreach (var fileInfo in di.EnumerateFiles()) fileNames.Add(fileInfo.FullName);
            foreach (var directoryInfo in di.EnumerateDirectories()) FillAllFileNames(directoryInfo, fileNames);
        }
    }
}