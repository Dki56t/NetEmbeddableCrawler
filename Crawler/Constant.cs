using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public static class Constant
    {
        public static HashSet<string> TxtFileExtensions = new HashSet<string>
        {
            ".txt",
            ".xml",
            ".js",
            ".css",
            ".less",
            ".sass"
        };

        public static HashSet<string> LinkItems = new HashSet<string>
        {
            "src",
            "href"
        };

        public static HashSet<string> BinaryNodes = new HashSet<string>
        {
            "img",
        };
    }
}
