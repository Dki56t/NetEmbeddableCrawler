using System;
using System.IO;
using System.Linq;

namespace Crawler.Logic
{
    public static class HtmlHelper
    {
        public static NodeType ResolveType(string nodeName, string url)
        {
            if (url.StartsWith("#"))
                return NodeType.Partial;
            if (Constant.BinaryNodes.Contains(nodeName))
                return NodeType.Binary;

            bool validUri = Uri.TryCreate(url, UriKind.Absolute, out var uri);
            var ext = validUri
                ? Path.GetExtension(uri.Segments.Last())
                : Path.GetExtension(url);

            if (url.StartsWith("mailto"))
                return NodeType.Mail;
            if (Constant.TxtFileExtensions.Contains(ext))
                return NodeType.Text;
            if (string.IsNullOrEmpty(ext))
                return NodeType.Html;
            return NodeType.Binary;
        }
    }

    public enum NodeType
    {
        Html,
        Text,
        Binary,
        Partial,
        Mail
    }
}
