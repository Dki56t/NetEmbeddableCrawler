using System.Net;
using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Integration
{
    [TestClass]
    public class FileLoaderTest
    {
        [TestMethod]
        public void TestSkipExceptions()
        {
            using (WebClient client = new WebClient())
            {
                FileLoader loader = new FileLoader(client);
                loader.LoadString("https://ru.linkedin.com");
            }
        }
    }
}
