namespace Crawler.Logic
{
    public interface IUrlMapper
    {
        string GetPath(string url, NodeType? nodeType = null);
    }
}