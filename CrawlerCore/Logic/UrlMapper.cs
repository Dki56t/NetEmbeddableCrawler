using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Crawler.Logic
{
    internal sealed class UrlMapper : IUrlMapper
    {
        private const string Index = "index.html";
        private const int MaxFileNameLength = 200;

        private static readonly Dictionary<char, string> ProhibitCharacters = new Dictionary<char, string>
        {
            {'?', "_p_"},
            {'%', "_pr_"},
            {'&', "_am_"},
            {'/', "_sl_"},
            {'|', "_ch_"},
            {'*', "_st_"},
            {':', "_dd_"},
            {'"', "_q_"},
            {'<', "_lt_"},
            {'>', "_gt_"},
        };

        /// <summary>
        ///     Contains map url from html (as a key) to path in file system.
        /// </summary>
        private readonly ConcurrentDictionary<string, string> _map = new ConcurrentDictionary<string, string>();

        private readonly string _outputDirectory;

        public UrlMapper(Configuration cfg)
        {
            _outputDirectory = cfg.DestinationFolder;
        }

        public string CreatePath(string url, NodeType? nodeType = null)
        {
            var normalizedUrl = UrlHelper.NormalizeUrl(url);
            if (_map.ContainsKey(normalizedUrl))
                return _map[normalizedUrl];

            var uri = new Uri(normalizedUrl);

            var hostUrl = UrlHelper.ExtractRoot(normalizedUrl);
            if (!_map.ContainsKey(hostUrl))
            {
                var hostPath = $"{_outputDirectory}\\{uri.Host}\\{Index}";
                _map.AddOrUpdate(hostUrl, _ => hostPath, (key, value) => value);

                if (hostUrl == normalizedUrl)
                    return _map[normalizedUrl];
            }
            
            var hostDirectoryPath = Path.GetDirectoryName(_map[hostUrl]);
            
            // SubUrl it is a part of url between host name and last segment (if last segment is file name).
            var subUrl = GetDirectoryName(uri);

            // If subUrl is empty, it won't insert into link.
            if (subUrl == null || subUrl == "\\")
                subUrl = string.Empty;
            
            var fileName = GetFileName(uri);
            var filePath = $"{hostDirectoryPath}{subUrl}\\{GetFileNameOrDefault(fileName, uri.Query, nodeType)}";

            _map.AddOrUpdate(normalizedUrl, _ => filePath, (key, value) => value);
            return _map[normalizedUrl];
        }

        public string GetPath(string url)
        {
            var normalizedUrl = UrlHelper.NormalizeUrl(url);
            return _map.ContainsKey(normalizedUrl) ? _map[normalizedUrl] : null;
        }

        private static string GetFileNameOrDefault(string fileName, string query, NodeType? nodeType)
        {
            var normalizedFileName = NormalizeStringForHtmlAndFileSystem(fileName);
            var normalizedQuery = NormalizeStringForHtmlAndFileSystem(query);
            if (normalizedQuery.Length > 100)
                normalizedQuery = $"_p_{Guid.NewGuid()}";

            if (string.IsNullOrEmpty(normalizedFileName) && string.IsNullOrEmpty(normalizedQuery))
                return Index;

            var extension = Path.GetExtension(normalizedFileName);
            if (!string.IsNullOrWhiteSpace(normalizedQuery) || string.IsNullOrEmpty(extension) ||
                nodeType == NodeType.Html)
                extension = ".html";

            // If query exists we save extension before query for file name.
            normalizedFileName =
                $"{(string.IsNullOrWhiteSpace(normalizedQuery) && nodeType != NodeType.Html ? Path.GetFileNameWithoutExtension(normalizedFileName) : Path.GetFileName(normalizedFileName))}" +
                $"{normalizedQuery}{extension}";

            return normalizedFileName;
        }

        /// <summary>
        ///     Replaces prohibit character with special string from static table.
        /// </summary>
        /// <returns>String with replaced prohibit characters.</returns>
        private static string NormalizeStringForHtmlAndFileSystem(string str)
        {
            StringBuilder result = null;

            for (var i = 0; i < str.Length; i++)
                if (ProhibitCharacters.ContainsKey(str[i]))
                {
                    result ??= new StringBuilder(str.Substring(0, i));
                    result.Append(ProhibitCharacters[str[i]]);
                }
                else
                {
                    result?.Append(str[i]);
                }

            return result?.ToString() ?? str;
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