using System;

namespace Crawler.Logic
{
    internal interface IUriMapper
    {
        string? CreatePath(Uri uri, NodeType? nodeType = null);
        string? GetPath(Uri uri);
    }
}