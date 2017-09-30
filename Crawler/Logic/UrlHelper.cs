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
            var partialIndex = url.IndexOf("/#", StringComparison.Ordinal);
            if (partialIndex > -1)
                url = url.Remove(partialIndex);
            partialIndex = url.IndexOf("#", StringComparison.Ordinal);
            if (partialIndex > -1)
                url = url.Remove(partialIndex);
            if (url.StartsWith("//"))
                url = $"https:{url}";
            if (url.EndsWith("/"))
                url = url.Remove(url.LastIndexOf("/", StringComparison.Ordinal));

            return url;
        }

        public static string GetPartialUrl(string url)
        {
            var index = url.LastIndexOf("/#", StringComparison.Ordinal);
            if (index > -1)
                return url.Substring(index);
            index = url.LastIndexOf("#", StringComparison.Ordinal);
            if (index > -1)
                return url.Substring(index);
            return string.Empty;
        }

        public static string ExtractRoot(string url)
        {
            var uri = new Uri(url);
            return uri.GetLeftPart(UriPartial.Authority);
        }

        public static string BuildRelativeUri(string root, string relative)
        {
            const string delimeter = "/";

            if (root.EndsWith(delimeter) && relative.StartsWith(delimeter))
                root = root.Remove(root.LastIndexOf(delimeter, StringComparison.Ordinal));
            if (root.EndsWith(delimeter) || relative.StartsWith(delimeter))
                return $"{root}{relative}";
            return $"{root}{delimeter}{relative}";
        }
    }
}
