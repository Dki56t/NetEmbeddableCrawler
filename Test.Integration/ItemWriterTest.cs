using System;
using System.Collections.Generic;
using System.IO;
using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Integration
{
    [TestClass]
    public class ItemWriterTest
    {
        [TestMethod]
        public void TestFileWrite()
        {
            string testDirectoryPath = Path.Combine(Path.GetTempPath(), "TestFileWrite");
            var di = new DirectoryInfo(testDirectoryPath);
            if (di.Exists)
                di.Delete(true);
            else
                di.Create();

            var item1= new Item("1", "http://site1/index.html");
            item1.AddItem(new Item("1/internal", "http://site1/internal/internal.html"));
            var item2 = new Item("2", "http://site2/index.html");
            item2.AddItem(new Item("2/internal", "http://site2/otherinternal/some.html"));

            ItemWriter.Write(new List<Item>
            {
                item1,
                item2
            }, di.FullName);
            var fileNames = new List<string>();
            FillAllFileNames(di, fileNames);

            Assert.AreEqual(fileNames.Count, 9);
            Assert.IsTrue(fileNames.Contains(testDirectoryPath));
            Assert.IsTrue(fileNames.Contains(Path.Combine(testDirectoryPath, "site1")));
            Assert.IsTrue(fileNames.Contains(Path.Combine(testDirectoryPath, "site1\\index.html")));
            Assert.IsTrue(fileNames.Contains(Path.Combine(testDirectoryPath, "site1\\internal")));
            Assert.IsTrue(fileNames.Contains(Path.Combine(testDirectoryPath, "site1\\internal\\internal.html")));
            Assert.IsTrue(fileNames.Contains(Path.Combine(testDirectoryPath, "site2")));
            Assert.IsTrue(fileNames.Contains(Path.Combine(testDirectoryPath, "site2\\index.html")));
            Assert.IsTrue(fileNames.Contains(Path.Combine(testDirectoryPath, "site2\\otherinternal")));
            Assert.IsTrue(fileNames.Contains(Path.Combine(testDirectoryPath, "site2\\otherinternal\\some.html")));
        }

        private void FillAllFileNames(DirectoryInfo di, List<string> fileNames)
        {
            fileNames.Add(di.FullName);
            foreach (var fileInfo in di.EnumerateFiles())
            {
                fileNames.Add(fileInfo.FullName);
            }
            foreach (var directoryInfo in di.EnumerateDirectories())
            {
                FillAllFileNames(directoryInfo, fileNames);
            }
        }
    }
}
