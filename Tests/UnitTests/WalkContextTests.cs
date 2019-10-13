using Crawler.Logic;
using HtmlAgilityPack;
using Xunit;

namespace Tests.UnitTests
{
    public class WalkContextTests
    {
        [Fact]
        public void ShouldTreatHttpsAndHttpUrlAsSame()
        {
            const string subUrl = "http://s.com/page";
            const string basicallySameSubUrl = "https://s.com/page";

            var doc = new HtmlDocument();
            doc.LoadHtml("<body>" +
                         $"<a href=\"{subUrl}\"> </a></body>");

            var context = new WalkContext("http://s.com", doc.DocumentNode);
            context.AddUrlIfMetFirstTime(subUrl, doc.DocumentNode);
            context.AddUrlIfMetFirstTime(basicallySameSubUrl, doc.DocumentNode);

            Assert.True(context.TryRequestContentProcessing(subUrl, doc.DocumentNode),
                "First request for content processing should be successful");
            Assert.False(context.TryRequestContentProcessing(subUrl, doc.DocumentNode),
                "Second request for content processing should be unsuccessful");
            Assert.False(context.TryRequestContentProcessing(basicallySameSubUrl, doc.DocumentNode),
                "First request for content processing (but with different scheme) should be unsuccessful");
        }
    }
}