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
            var subUri = "http://s.com/page".AsUri();
            var basicallySameSubUri = "https://s.com/page".AsUri();

            var doc = new HtmlDocument();
            doc.LoadHtml("<body>" +
                         $"<a href=\"{subUri.OriginalString}\"> </a></body>");

            var context = new WalkContext("http://s.com".AsUri());

            context.TryRequestContentProcessing(subUri)
                .ShouldBeTrue("First request for content processing should be successful");
            context.TryRequestContentProcessing(subUri)
                .ShouldBeFalse("Second request for content processing should be unsuccessful");
            context.TryRequestContentProcessing(basicallySameSubUri)
                .ShouldBeFalse("First request for content processing " +
                               "(but with different scheme) should be unsuccessful");
        }
    }
}