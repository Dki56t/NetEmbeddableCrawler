using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Crawler.Logic;
using Moq;
using Xunit;

namespace Tests.IntegrationTests
{
    public class ItemWriterTests
    {
        public ItemWriterTests()
        {
            _testDirectoryPath = Path.Combine(Path.GetTempPath(), "TestFileWrite");
            var directory = new DirectoryInfo(_testDirectoryPath);

            if (directory.Exists)
                directory.Delete(true);
            else
                directory.Create();
        }

        private readonly string _testDirectoryPath;

        private static void FillAllFileNames(DirectoryInfo di, ICollection<string> fileNames)
        {
            fileNames.Add(di.FullName);
            foreach (var fileInfo in di.EnumerateFiles()) fileNames.Add(fileInfo.FullName);
            foreach (var directoryInfo in di.EnumerateDirectories()) FillAllFileNames(directoryInfo, fileNames);
        }

        [Fact]
        public async Task ShouldWriteItemsToFileSystem()
        {
            var item1 = new Item("http://site1/index.html");
            var item11 = new Item("http://site1/internal/internal.html");
            var item2 = new Item("http://site2/index.html");
            var item21 = new Item("http://site2/other_internal/some.html");

            var mapper = new Mock<IUrlMapper>();
            mapper.Setup(x => x.CreatePath(item1.Uri, null))
                .Returns(Path.Combine(_testDirectoryPath, "site1\\index.html"));
            mapper.Setup(x => x.CreatePath(item11.Uri, null))
                .Returns(Path.Combine(_testDirectoryPath, "site1\\internal\\internal.html"));
            mapper.Setup(x => x.CreatePath(item2.Uri, null))
                .Returns(Path.Combine(_testDirectoryPath, "site2\\index.html"));
            mapper.Setup(x => x.CreatePath(item21.Uri, null))
                .Returns(Path.Combine(_testDirectoryPath, "site2\\other_internal\\some.html"));

            var writer = new ItemWriter(mapper.Object);
            await writer.Write(item1).ConfigureAwait(false);
            await writer.Write(item11).ConfigureAwait(false);
            await writer.Write(item21).ConfigureAwait(false);
            await writer.Write(item2).ConfigureAwait(false);

            var fileNames = new List<string>();
            FillAllFileNames(new DirectoryInfo(_testDirectoryPath), fileNames);

            Assert.Equal(9, fileNames.Count);
            Assert.Contains(_testDirectoryPath, fileNames);
            Assert.Contains(Path.Combine(_testDirectoryPath, "site1"), fileNames);
            Assert.Contains(Path.Combine(_testDirectoryPath, "site1\\index.html"), fileNames);
            Assert.Contains(Path.Combine(_testDirectoryPath, "site1\\internal"), fileNames);
            Assert.Contains(Path.Combine(_testDirectoryPath, "site1\\internal\\internal.html"), fileNames);
            Assert.Contains(Path.Combine(_testDirectoryPath, "site2"), fileNames);
            Assert.Contains(Path.Combine(_testDirectoryPath, "site2\\index.html"), fileNames);
            Assert.Contains(Path.Combine(_testDirectoryPath, "site2\\other_internal"), fileNames);
            Assert.Contains(Path.Combine(_testDirectoryPath, "site2\\other_internal\\some.html"), fileNames);
        }
    }
}