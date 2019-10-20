namespace Crawler.Logic
{
    internal interface IUrlMapper
    {
        string CreatePath(string url, NodeType? nodeType = null);
        string GetPath(string url);
    }
}