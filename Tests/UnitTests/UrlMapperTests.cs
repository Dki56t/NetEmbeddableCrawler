using System;
using System.IO;
using System.Linq;
using Crawler;
using Crawler.Logic;
using Crawler.Projections;
using Xunit;

// ReSharper disable StringLiteralTypo - some urls are very specific

namespace Tests.UnitTests
{
    public class UrlMapperTests
    {
        public UrlMapperTests()
        {
            _testDirectoryPath = Path.Combine(Path.GetTempPath(), "TestFileWrite");
        }

        private readonly string _testDirectoryPath;

        [Fact]
        public void RMapUrlWithParameters()
        {
            var mapper = new UrlMapper(new Configuration
            {
                DestinationDirectory = _testDirectoryPath
            });

            Assert.Equal(Path.Combine(_testDirectoryPath,
                    @"site1\_ch_.html"),
                mapper.CreatePath("http://site1/|"));

            Assert.Equal(Path.Combine(_testDirectoryPath,
                    @"site1\index.html"),
                mapper.CreatePath("http://site1/"));

            Assert.Equal(Path.Combine(_testDirectoryPath,
                    @"site1\issues_p_q=is_pr_3Aopen+is_pr_3Aissue.html"),
                mapper.CreatePath("http://site1/issues?q=is%3Aopen+is%3Aissue"));

            Assert.Equal(Path.Combine(_testDirectoryPath,
                    @"site1\issues_p_q=is_pr_3Aissue+is_pr_3Aclosed.html"),
                mapper.CreatePath("http://site1/issues?q=is%3Aissue+is%3Aclosed"));

            Assert.Equal(Path.Combine(_testDirectoryPath,
                    @"site1\issues_p_q=is_pr_3Aissue+is_pr_3Aopen+sort_pr_3Acreated-asc.html"),
                mapper.CreatePath("http://site1/issues?q=is%3Aissue+is%3Aopen+sort%3Acreated-asc"));

            Assert.Equal(Path.Combine(_testDirectoryPath,
                    @"site1\issues_p_q=is_pr_3Aissue+is_pr_3Aopen+sort_pr_3Acomments-desc.html"),
                mapper.CreatePath("http://site1/issues?q=is%3Aissue+is%3Aopen+sort%3Acomments-desc"));

            Assert.Equal(Path.Combine(_testDirectoryPath,
                    @"site1\issues.html"),
                mapper.CreatePath("http://site1/issues"));

            Assert.Equal(Path.Combine(_testDirectoryPath,
                    @"site1\sharer.php_p_u=http_pr_3A_pr_2F_pr_2Fhtml-agility-pack.net_pr_2F.html"),
                mapper.CreatePath("http://site1/sharer.php?u=http%3A%2F%2Fhtml-agility-pack.net%2F"));

            Assert.Equal(Path.Combine(_testDirectoryPath,
                    @"site1.net\_p_ref=topbar_help.html"),
                mapper.CreatePath("http://site1.net/?ref=topbar_help"));

            Assert.Equal(Path.Combine(_testDirectoryPath,
                    @"site1.net\_p__am__pr_25.html"),
                mapper.CreatePath("http://site1.net/?&%25"));
        }

        [Fact]
        public void ShouldAppendExtensionToFileName()
        {
            var mapper = new UrlMapper(new Configuration
            {
                DestinationDirectory = _testDirectoryPath
            });

            Assert.Equal(Path.Combine(_testDirectoryPath, @"site1\index.html"),
                mapper.CreatePath("http://site1/"));
            Assert.Equal(Path.Combine(_testDirectoryPath, @"site1\test\some.html"),
                mapper.CreatePath("http://site1/test/some"));
            Assert.Equal(Path.Combine(_testDirectoryPath, @"site1\css\style.css"),
                mapper.CreatePath("http://site1/css/style.css"));
            Assert.Equal(Path.Combine(_testDirectoryPath, @"site1\test\1.5.5.html"),
                mapper.CreatePath("http://site1/test/1.5.5", NodeType.Html));
        }

        [Fact]
        public void ShouldMapPartialUri()
        {
            var mapper = new UrlMapper(new Configuration
            {
                DestinationDirectory = _testDirectoryPath
            });

            Assert.Equal(Path.Combine(_testDirectoryPath, @"site4\index.html"),
                mapper.CreatePath("http://site4/#"));
            Assert.Equal(Path.Combine(_testDirectoryPath, @"site4\test\some.html"),
                mapper.CreatePath("http://site4/test/some.html/#part-2"));
        }

        [Fact]
        public void ShouldMapRelativeUri()
        {
            var mapper = new UrlMapper(new Configuration
            {
                DestinationDirectory = _testDirectoryPath
            });

            Assert.Equal(Path.Combine(_testDirectoryPath, @"site1\index.html"),
                mapper.CreatePath("http://site1/index.html"));

            Assert.Equal(Path.Combine(_testDirectoryPath, @"site1\internal\internal.html"),
                mapper.CreatePath("http://site1/internal/internal.html"));

            Assert.Equal(Path.Combine(_testDirectoryPath, @"site2\index2.html"),
                mapper.CreatePath("http://site2/index2.html"));

            Assert.Equal(Path.Combine(_testDirectoryPath, @"site2\other_internal\some.html"),
                mapper.CreatePath("http://site2/other_internal/some.html"));
        }

        [Fact]
        public void ShouldMapUriWithoutDirectNestedDirectory()
        {
            var mapper = new UrlMapper(new Configuration
            {
                DestinationDirectory = _testDirectoryPath
            });

            Assert.Equal(Path.Combine(_testDirectoryPath, @"site3\index.html"),
                mapper.CreatePath("http://site3/"));
            Assert.Equal(Path.Combine(_testDirectoryPath, @"site3\test\some.html"),
                mapper.CreatePath("http://site3/test/some.html/"));
        }

        [Fact]
        public void ShouldOmitQueryParametersInCssOrJsUrls()
        {
            var mapper = new UrlMapper(new Configuration
            {
                DestinationDirectory = _testDirectoryPath
            });

            Assert.Equal(Path.Combine(_testDirectoryPath, @"site1\min.css"),
                mapper.CreatePath("http://site1/min.css?v=12345678"));
        }

        [Fact]
        public void ShouldRecognizeHostName()
        {
            var item1 = new Item("http://site1/test/some");

            var mapper = new UrlMapper(new Configuration
            {
                DestinationDirectory = _testDirectoryPath
            });
            mapper.CreatePath(item1.Uri);

            Assert.Equal(Path.Combine(_testDirectoryPath, @"site1\test\some.html"),
                mapper.CreatePath("http://site1/test/some"));
            Assert.Equal(Path.Combine(_testDirectoryPath, @"site1\index.html"),
                mapper.CreatePath("http://site1/"));
        }

        [Fact]
        public void ShouldReplaceLongDirectoryNamePath()
        {
            var longUrl = $"http://url/{string.Join("", Enumerable.Range(0, 200))}/img.jpeg";
            var mapper = new UrlMapper(new Configuration
            {
                DestinationDirectory = _testDirectoryPath
            });

            Assert.Equal(_testDirectoryPath.Length + "url".Length + 2 * (@"\" + Guid.Empty).Length + ".jpeg".Length,
                mapper.CreatePath(longUrl).Length);
        }

        [Fact]
        public void ShouldReplaceLongFileNamePath()
        {
            var longUrl = $"http://url/{string.Join("", Enumerable.Range(0, 200))}.jpeg";
            var item1 = new Item(longUrl);

            var mapper = new UrlMapper(new Configuration
            {
                DestinationDirectory = _testDirectoryPath
            });
            mapper.CreatePath(item1.Uri);

            Assert.Equal(_testDirectoryPath.Length + "url".Length + 2 * (@"\" + Guid.Empty).Length + ".jpeg".Length,
                mapper.CreatePath(longUrl).Length);
        }

        [Fact]
        public void ShouldReplaceLongQueryPath()
        {
            var longUrl = $"http://url/?{string.Join("", Enumerable.Range(0, 200))}";
            var mapper = new UrlMapper(new Configuration
            {
                DestinationDirectory = _testDirectoryPath
            });

            Assert.Equal(_testDirectoryPath.Length + @"url\_p_".Length + (@"\" + Guid.Empty).Length + ".html".Length,
                mapper.CreatePath(longUrl).Length);
        }
    }
}