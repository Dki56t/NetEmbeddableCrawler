using System.Collections.Generic;

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

        public static HashSet<string> BinaryFileExtensions = new HashSet<string>
        {
            ".jpeg"
        };

        public static HashSet<string> LinkItems = new HashSet<string>
        {
            "src",
            "href"
        };

        public static HashSet<string> CrossOriginItems = new HashSet<string>
        {
            "integrity",
            "crossorigin"
        };

        public static HashSet<string> BinaryNodes = new HashSet<string>
        {
            "img"
        };

        public static HashSet<string> HtmlNodes = new HashSet<string>
        {
            "a"
        };
    }
}