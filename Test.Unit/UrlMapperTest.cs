using System;
using System.IO;
using Crawler;
using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Unit
{
    
    public class UrlMapperTest
    {
        private string _testDirectoryPath;

        [TestInitialize]
        public void Init()
        {
            _testDirectoryPath = Path.Combine(Path.GetTempPath(), "TestFileWrite");
        }

        [Fact]
        public void TestMapping()
        {
            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });

            Assert.AreEqual(mapper.GetPath("http://site1/index.html"),
                Path.Combine(_testDirectoryPath, "site1\\index.html"));

            Assert.AreEqual(mapper.GetPath("http://site1/internal/internal.html"),
                Path.Combine(_testDirectoryPath, "site1\\internal\\internal.html"));

            Assert.AreEqual(mapper.GetPath("http://site2/index2.html"),
                Path.Combine(_testDirectoryPath, "site2\\index2.html"));

            Assert.AreEqual(mapper.GetPath("http://site2/otherinternal/some.html"),
                Path.Combine(_testDirectoryPath, "site2\\otherinternal\\some.html"));
        }

        [Fact]
        public void TestMappingPartial()
        {
            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });

            Assert.AreEqual(mapper.GetPath("http://site4/#"), Path.Combine(_testDirectoryPath, "site4\\index.html"));
            Assert.AreEqual(mapper.GetPath("http://site4/test/some.html/#part-2"),
                Path.Combine(_testDirectoryPath, "site4\\test\\some.html"));
        }

        [Fact]
        public void TestMappingAbnormal()
        {
            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });

            Assert.AreEqual(mapper.GetPath("http://site3/"), Path.Combine(_testDirectoryPath, "site3\\index.html"));
            Assert.AreEqual(mapper.GetPath("http://site3/test/some.html/"),
                Path.Combine(_testDirectoryPath, "site3\\test\\some.html"));
        }

        [Fact]
        public void TestFileNameAppendExtension()
        {
            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });

            Assert.AreEqual(mapper.GetPath("http://site1/"), Path.Combine(_testDirectoryPath, "site1\\index.html"));
            Assert.AreEqual(mapper.GetPath("http://site1/test/some"),
                Path.Combine(_testDirectoryPath, "site1\\test\\some.html"));
            Assert.AreEqual(mapper.GetPath("http://site1/css/style.css"),
                Path.Combine(_testDirectoryPath, "site1\\css\\style.css"));
            Assert.AreEqual(mapper.GetPath("http://site1/test/1.5.5", NodeType.Html),
                Path.Combine(_testDirectoryPath, "site1\\test\\1.5.5.html"));
        }

        [Fact]
        public void TestUrlWithParameters()
        {
            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });

            Assert.AreEqual(mapper.GetPath("http://site1/"),
                Path.Combine(_testDirectoryPath, "site1\\index.html"));

            Assert.AreEqual(mapper.GetPath("http://site1/issues?q=is%3Aopen+is%3Aissue"),
                Path.Combine(_testDirectoryPath, "site1\\issues_p_q=is_pr_3Aopen+is_pr_3Aissue.html"));

            Assert.AreEqual(mapper.GetPath("http://site1/issues?q=is%3Aissue+is%3Aclosed"),
                Path.Combine(_testDirectoryPath, "site1\\issues_p_q=is_pr_3Aissue+is_pr_3Aclosed.html"));

            Assert.AreEqual(mapper.GetPath("http://site1/issues?q=is%3Aissue+is%3Aopen+sort%3Acreated-asc"),
                Path.Combine(_testDirectoryPath,
                    "site1\\issues_p_q=is_pr_3Aissue+is_pr_3Aopen+sort_pr_3Acreated-asc.html"));

            Assert.AreEqual(mapper.GetPath("http://site1/issues?q=is%3Aissue+is%3Aopen+sort%3Acomments-desc"),
                Path.Combine(_testDirectoryPath,
                    "site1\\issues_p_q=is_pr_3Aissue+is_pr_3Aopen+sort_pr_3Acomments-desc.html"));

            Assert.AreEqual(mapper.GetPath("http://site1/issues"),
                Path.Combine(_testDirectoryPath, "site1\\issues.html"));

            Assert.AreEqual(mapper.GetPath("http://site1/sharer.php?u=http%3A%2F%2Fhtml-agility-pack.net%2F"),
                Path.Combine(_testDirectoryPath,
                    "site1\\sharer.php_p_u=http_pr_3A_pr_2F_pr_2Fhtml-agility-pack.net_pr_2F.html"));

            Assert.AreEqual(mapper.GetPath("http://site1.net/?ref=topbar_help"),
                Path.Combine(_testDirectoryPath, "site1.net\\_p_ref=topbar_help.html"));

            Assert.AreEqual(mapper.GetPath("http://site1.net/?&%25"),
                Path.Combine(_testDirectoryPath, "site1.net\\_p__am__pr_25.html"));
        }

        [Fact]
        public void TestHostNameRecognition()
        {
            var item1 = new Item("1", "http://site1/test/some");

            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });
            mapper.GetPath(item1.Uri);

            Assert.AreEqual(mapper.GetPath("http://site1/test/some"),
                Path.Combine(_testDirectoryPath, "site1\\test\\some.html"));
            Assert.AreEqual(mapper.GetPath("http://site1/"), Path.Combine(_testDirectoryPath, "site1\\index.html"));
        }

        [Fact]
        public void TestLongFileNamePath()
        {
            var longUrl = "http://site1/test/some/123456789012345678901234567890123456789012345678901234567890" +
                          "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                          "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                          "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                          "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                          "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890.jpeg";
            var item1 = new Item("1", longUrl);

            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });
            mapper.GetPath(item1.Uri);

            Assert.AreEqual(mapper.GetPath(longUrl).Length,
                _testDirectoryPath.Length + "site1".Length + 2 * ("\\" + Guid.Empty).Length + ".jpeg".Length);
        }

        [Fact]
        public void TestLongDirectoryNamePath()
        {
            var longUrl = "http://site1/test/some/123456789012345678901234567890123456789012345678901234567890" +
                          "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                          "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                          "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                          "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                          "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890/file.jpeg";

            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });

            Assert.AreEqual(mapper.GetPath(longUrl).Length,
                _testDirectoryPath.Length + "site1".Length + 2 * ("\\" + Guid.Empty).Length + ".jpeg".Length);
        }

        [Fact]
        public void TestLongQueryPath()
        {
            var longUrl = "http://site1/test/some/?123456789012345678901234567890123456789012345678901234567890" +
                          "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                          "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                          "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                          "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                          "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";

            var mapper = new UrlMapper(new Configuration
            {
                DestinationFolder = _testDirectoryPath
            });

            Assert.AreEqual(mapper.GetPath(longUrl).Length,
                _testDirectoryPath.Length + "site1/test/some/_p_".Length + ("\\" + Guid.Empty).Length + ".html".Length);
        }
    }
}