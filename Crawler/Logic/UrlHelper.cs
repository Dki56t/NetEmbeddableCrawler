using System;

namespace Crawler.Logic
{
    public static class UrlHelper
    {
        public static bool IsExternalLink(string url)
        {
            return url.StartsWith("//") || url.StartsWith("http");
        }

        public static string NormalizeUrl(string url)
        {
            if (url.StartsWith("//"))
                url = $"https:{url}";
            if (url.EndsWith("/"))
                url = url.Remove(url.LastIndexOf("/", StringComparison.Ordinal));

            return url;
        }

        public static string ExtractRoot(string url)
        {
            var uri = new Uri(url);
            return uri.GetLeftPart(UriPartial.Authority);
        }
    }
}
