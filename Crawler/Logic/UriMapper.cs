using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Crawler.Logic
{
    internal sealed class UriMapper : IUriMapper
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
            {'>', "_gt_"}
        };

        /// <summary>
        ///     Contains map from uri (as a key) to path in file system.
        /// </summary>
        private readonly ConcurrentDictionary<Uri, string> _map = new ConcurrentDictionary<Uri, string>(new UriComparer());

        private readonly string _outputDirectory;

        public UriMapper(Configuration cfg)
        {
            _outputDirectory = cfg.DestinationDirectory;
        }

        public string? CreatePath(Uri uri, NodeType? nodeType = null)
        {
            var normalizedUri = UrlHelper.NormalizeUrl(uri.OriginalString);
            if (normalizedUri == null)
                return null;

            uri = normalizedUri;
            if (_map.ContainsKey(uri))
                return _map[uri];

            var extension = Path.GetExtension(uri.LocalPath);
            if (!string.IsNullOrWhiteSpace(uri.Query) && Constant.TxtFileExtensions.Contains(extension))
                uri = uri.AbsoluteUri
                    .Remove(uri.AbsoluteUri.IndexOf(uri.Query, StringComparison.InvariantCultureIgnoreCase)).AsUri();

            var hostUri = UrlHelper.ExtractRoot(uri);
            if (!_map.ContainsKey(hostUri))
            {
                var hostPath = $"{_outputDirectory}\\{uri.Host}\\{Index}";
                _map.AddOrUpdate(hostUri, _ => hostPath, (key, value) => value);

                if (hostUri == uri)
                    return _map[uri];
            }

            var hostDirectoryPath = Path.GetDirectoryName(_map[hostUri]);

            // Directory name is a part of uri between host name and last segment (if last segment is file name).
            var directory = GetDirectoryName(uri);

            // If directory name is empty, it won't be inserted into link.
            if (directory == null || directory == "\\")
                directory = string.Empty;

            var fileName = GetFileName(uri);
            var filePath = $"{hostDirectoryPath}{directory}\\{GetFileNameOrDefault(fileName, uri.Query, nodeType)}";

            _map.AddOrUpdate(uri, _ => filePath, (key, value) => value);
            return _map[uri];
        }

        public string? GetPath(Uri uri)
        {
            return _map.ContainsKey(uri) ? _map[uri] : null;
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
            StringBuilder? result = null;

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

        private static string? GetDirectoryName(Uri uri)
        {
            var directoryName = uri.LocalPath.Length > MaxFileNameLength
                ? Guid.NewGuid().ToString()
                : Path.GetDirectoryName(uri.LocalPath);

            return directoryName;
        }
    }
}