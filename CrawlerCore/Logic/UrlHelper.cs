using System;

namespace Crawler.Logic
{
    internal static class UrlHelper
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

            return !Uri.TryCreate(url, UriKind.Absolute, out _) ? null : url;
        }

        public static string GetPartialUrl(string url)
        {
            var index = url.LastIndexOf("/#", StringComparison.Ordinal);
            if (index > -1)
                return url.Substring(index);
            index = url.LastIndexOf("#", StringComparison.Ordinal);
            return index > -1 ? url.Substring(index) : string.Empty;
        }

        public static string ExtractRoot(string url)
        {
            var uri = new Uri(url);
            return uri.GetLeftPart(UriPartial.Authority);
        }

        public static string BuildRelativeUri(string root, string relative)
        {
            const string delimiter = "/";

            if (root.EndsWith(delimiter) && relative.StartsWith(delimiter))
                root = root.Remove(root.LastIndexOf(delimiter, StringComparison.Ordinal));
            if (root.EndsWith(delimiter) || relative.StartsWith(delimiter))
                return $"{root}{relative}";
            return $"{root}{delimiter}{relative}";
        }

        public static bool EqualHosts(string first, string second)
        {
            var newRootUri = new Uri(first);
            var currentRootUri = new Uri(second);

            return Uri.Compare(currentRootUri, newRootUri, UriComponents.Host, UriFormat.SafeUnescaped,
                       StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}