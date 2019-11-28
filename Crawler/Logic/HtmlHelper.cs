using System;
using System.IO;
using System.Linq;

namespace Crawler.Logic
{
    internal static class HtmlHelper
    {
        public static NodeType ResolveType(string nodeName, string url)
        {
            if (url.StartsWith("#"))
                return NodeType.Partial;
            if (Constant.BinaryNodes.Contains(nodeName))
                return NodeType.Binary;
            if (url.StartsWith("mailto"))
                return NodeType.Mail;

            var validUri = Uri.TryCreate(url, UriKind.Absolute, out var uri);

            string ext;
            if (validUri)
            {
                if (uri!.Segments.Length == 0)
                    throw new InvalidOperationException($"Invalid url ({url})");
                ext = Path.GetExtension(uri.Segments.Last());
            }
            else
            {
                ext = Path.GetExtension(url);
            }

            if (Constant.TxtFileExtensions.Contains(ext))
                return NodeType.Text;
            if (Constant.BinaryFileExtensions.Contains(ext))
                return NodeType.Binary;
            if (string.IsNullOrEmpty(ext) || Constant.HtmlNodes.Contains(nodeName))
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