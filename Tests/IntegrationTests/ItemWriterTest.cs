using System.Collections.Generic;
using System.IO;
using Crawler.Logic;
using Moq;
using Xunit;

namespace Tests.IntegrationTests
{
    public class ItemWriterTest
    {
        public ItemWriterTest()
        {
            _testDirectoryPath = Path.Combine(Path.GetTempPath(), "TestFileWrite");
            var di = new DirectoryInfo(_testDirectoryPath);
            if (di.Exists)
                di.Delete(true);
            else
                di.Create();
        }

        private readonly string _testDirectoryPath;

        private static void FillAllFileNames(DirectoryInfo di, List<string> fileNames)
        {
            fileNames.Add(di.FullName);
            foreach (var fileInfo in di.EnumerateFiles()) fileNames.Add(fileInfo.FullName);
            foreach (var directoryInfo in di.EnumerateDirectories()) FillAllFileNames(directoryInfo, fileNames);
        }

        [Fact]
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

            var mapper = new Mock<IUrlMapper>();
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
            Assert.Equal(fileNames.Count, 9);
            Assert.True(fileNames.Contains(_testDirectoryPath));
            Assert.True(fileNames.Contains(Path.Combine(_testDirectoryPath, "site1")));
            Assert.True(fileNames.Contains(Path.Combine(_testDirectoryPath, "site1\\index.html")));
            Assert.True(fileNames.Contains(Path.Combine(_testDirectoryPath, "site1\\internal")));
            Assert.True(fileNames.Contains(Path.Combine(_testDirectoryPath, "site1\\internal\\internal.html")));
            Assert.True(fileNames.Contains(Path.Combine(_testDirectoryPath, "site2")));
            Assert.True(fileNames.Contains(Path.Combine(_testDirectoryPath, "site2\\index.html")));
            Assert.True(fileNames.Contains(Path.Combine(_testDirectoryPath, "site2\\otherinternal")));
            Assert.True(fileNames.Contains(Path.Combine(_testDirectoryPath, "site2\\otherinternal\\some.html")));
        }
    }
}