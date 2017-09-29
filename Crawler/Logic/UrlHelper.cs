namespace Crawler.Logic
{
    public static class UrlHelper
    {
        public static bool IsExternalLink(string url)
        {
            return url.StartsWith("//") || url.StartsWith("http");
        }
    }
}
