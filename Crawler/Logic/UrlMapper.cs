using System;
using System.Collections.Concurrent;
using System.IO;

namespace Crawler.Logic
{
    internal class UrlMapper
    {
        private const string Index = "index.html";
        private const int MaxFileNameLength = 200;

        /// <summary>
        ///     Contains map url from html (as a key) to path in file system
        /// </summary>
        private readonly ConcurrentDictionary<string, string> _map = new ConcurrentDictionary<string, string>();

        private readonly string _outputDirectory;

        public UrlMapper(Configuration cfg)
        {
            _outputDirectory = cfg.DestinationFolder;
        }

        public virtual string GetPath(string url, NodeType? nodeType = null)
        {
            var normalizedUrl = UrlHelper.NormalizeUrl(url);
            if (_map.ContainsKey(normalizedUrl))
                return _map[normalizedUrl];

            var uri = new Uri(normalizedUrl);
            var fileName = GetFileName(uri);
            //subUrl it is a part of url between host name and last segment (if last segment is file name)
            var subUrl = GetDirectoryName(uri);

            //if subUrl is empty, it won't inserted into link
            if (subUrl == null || subUrl == "\\")
                subUrl = string.Empty;

            //we need path to host directory for all links
            var hostUrl = UrlHelper.ExtractRoot(normalizedUrl);
            if (!_map.ContainsKey(hostUrl))
            {
                var hostPath = $"{_outputDirectory}\\{uri.Host}\\{Index}";
                _map.AddOrUpdate(hostUrl, _ => hostPath, (key, value) => value);

                if (hostUrl == normalizedUrl)
                    return _map[normalizedUrl];
            }

            var hostDirectoryPath = Path.GetDirectoryName(_map[hostUrl]);
            var filePath = $"{hostDirectoryPath}{subUrl}\\{GetFileNameOrDefault(fileName, uri.Query, nodeType)}";

            _map.AddOrUpdate(normalizedUrl, _ => filePath, (key, value) => value);
            return _map[normalizedUrl];
        }

        private static string GetFileNameOrDefault(string fileName, string query, NodeType? nodeType)
        {
            //incude query string manually for applying file system and browser
            var normalizedQuery = query
                .Replace("?", "_p_")
                .Replace("%", "_pr_")
                .Replace("&", "_am_")
                .Replace("/", "_sl_");
            if (normalizedQuery.Length > 100)
                normalizedQuery = $"_p_{Guid.NewGuid()}";
            if (string.IsNullOrEmpty(fileName) && string.IsNullOrEmpty(normalizedQuery))
                return Index;
            var extension = Path.GetExtension(fileName);
            if (!string.IsNullOrWhiteSpace(normalizedQuery) || string.IsNullOrEmpty(extension) ||
                nodeType == NodeType.Html)
                extension = ".html";
            //if query exists we save extension before query for file name
            fileName =
                $"{(string.IsNullOrWhiteSpace(normalizedQuery) && nodeType != NodeType.Html ? Path.GetFileNameWithoutExtension(fileName) : Path.GetFileName(fileName))}" +
                $"{normalizedQuery}{extension}";

            return fileName;
        }

        private static string GetFileName(Uri uri)
        {
            var fileName = uri.LocalPath.Length > MaxFileNameLength
                ? $"{Guid.NewGuid().ToString()}{Path.GetExtension(uri.LocalPath)}"
                : Path.GetFileName(uri.LocalPath);

            return fileName;
        }

        private static string GetDirectoryName(Uri uri)
        {
            var directoryName = uri.LocalPath.Length > MaxFileNameLength
                ? Guid.NewGuid().ToString()
                : Path.GetDirectoryName(uri.LocalPath);

            return directoryName;
        }
    }
}