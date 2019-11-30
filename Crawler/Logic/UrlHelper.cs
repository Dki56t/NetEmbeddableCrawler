using System;

namespace Crawler.Logic
{
    internal static class UrlHelper
    {
        public static bool IsAbsoluteUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                   !string.Equals(uri.Scheme, "data", StringComparison.OrdinalIgnoreCase);
        }

        public static Uri? NormalizeUrl(string url)
        {
            var fragmentIndex = url.IndexOf("#", StringComparison.Ordinal);
            if (fragmentIndex > -1)
                url = url.Remove(fragmentIndex);
            if (url.StartsWith("//"))
                url = $"https:{url}";
            if (url.EndsWith("/"))
                url = url.Remove(url.LastIndexOf("/", StringComparison.Ordinal));

            return !Uri.TryCreate(url, UriKind.Absolute, out var uri) ? null : uri;
        }

        public static string GetFragmentComponent(Uri uri)
        {
            return uri.GetComponents(UriComponents.Fragment, UriFormat.Unescaped);
        }

        public static Uri ExtractRoot(Uri uri)
        {
            return uri.GetLeftPart(UriPartial.Authority).AsUri();
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

        public static bool EqualHosts(Uri first, Uri second)
        {
            return Uri.Compare(first, second, UriComponents.Host, UriFormat.SafeUnescaped,
                       StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static Uri AsUri(this string originalString)
        {
            return new Uri(originalString, UriKind.RelativeOrAbsolute);
        }
    }
}