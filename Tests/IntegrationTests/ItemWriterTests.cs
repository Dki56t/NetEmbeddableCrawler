using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Crawler.Logic;
using Crawler.Projections;
using Moq;
using Shouldly;
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
            var item1 = new Item("http://site1/index.html".AsUri());
            var item11 = new Item("http://site1/internal/internal.html".AsUri());
            var item2 = new Item("http://site2/index.html".AsUri());
            var item21 = new Item("http://site2/other_internal/some.html".AsUri());

            var mapper = new Mock<IUriMapper>();
            mapper.Setup(x => x.CreatePath(item1.Uri, null))
                .Returns(Path.Combine(_testDirectoryPath, "site1\\index.html"));
            mapper.Setup(x => x.CreatePath(item11.Uri, null))
                .Returns(Path.Combine(_testDirectoryPath, "site1\\internal\\internal.html"));
            mapper.Setup(x => x.CreatePath(item2.Uri, null))
                .Returns(Path.Combine(_testDirectoryPath, "site2\\index.html"));
            mapper.Setup(x => x.CreatePath(item21.Uri, null))
                .Returns(Path.Combine(_testDirectoryPath, "site2\\other_internal\\some.html"));

            var writer = new ItemWriter(mapper.Object);
            await writer.WriteAsync(item1).ConfigureAwait(false);
            await writer.WriteAsync(item11).ConfigureAwait(false);
            await writer.WriteAsync(item21).ConfigureAwait(false);
            await writer.WriteAsync(item2).ConfigureAwait(false);

            var fileNames = new List<string>();
            FillAllFileNames(new DirectoryInfo(_testDirectoryPath), fileNames);

            fileNames.Count.ShouldBe(9);
            fileNames.ShouldContain(_testDirectoryPath);
            fileNames.ShouldContain(Path.Combine(_testDirectoryPath, "site1"));
            fileNames.ShouldContain(Path.Combine(_testDirectoryPath, "site1\\index.html"));
            fileNames.ShouldContain(Path.Combine(_testDirectoryPath, "site1\\internal"));
            fileNames.ShouldContain(Path.Combine(_testDirectoryPath, "site1\\internal\\internal.html"));
            fileNames.ShouldContain(Path.Combine(_testDirectoryPath, "site2"));
            fileNames.ShouldContain(Path.Combine(_testDirectoryPath, "site2\\index.html"));
            fileNames.ShouldContain(Path.Combine(_testDirectoryPath, "site2\\other_internal"));
            fileNames.ShouldContain(Path.Combine(_testDirectoryPath, "site2\\other_internal\\some.html"));
        }
    }
}