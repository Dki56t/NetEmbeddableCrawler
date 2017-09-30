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
            FileLoader loader = new FileLoader();
            await loader.LoadString("https://ru.linkedin.com");
        }
    }
}
