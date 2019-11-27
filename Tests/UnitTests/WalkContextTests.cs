using Crawler.Logic;
using HtmlAgilityPack;
using Shouldly;
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

            var context = new WalkContext("http://s.com");

            context.TryRequestContentProcessing(subUrl)
                .ShouldBeTrue("First request for content processing should be successful");
            context.TryRequestContentProcessing(subUrl)
                .ShouldBeFalse("Second request for content processing should be unsuccessful");
            context.TryRequestContentProcessing(basicallySameSubUrl)
                .ShouldBeFalse("First request for content processing " +
                               "(but with different scheme) should be unsuccessful");
        }
    }
}