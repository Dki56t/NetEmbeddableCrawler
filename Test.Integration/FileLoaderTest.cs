using System.Threading.Tasks;
using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Integration
{
    [TestClass]
    public class FileLoaderTest
    {
        [TestMethod]
        public async Task TestSkipExceptions()
        {
            var loader = new FileLoader();
            await loader.LoadString("https://ru.linkedin.com");
        }
    }
}