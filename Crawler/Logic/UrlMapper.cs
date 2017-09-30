using System;
using System.Collections.Generic;
using System.IO;

namespace Crawler.Logic
{
    internal class UrlMapper
    {
        private readonly string _outputDirectory;

        /// <summary>
        /// Contains map url from html (as a key) to path in file system
        /// </summary>
        private readonly Dictionary<string, string> _map = new Dictionary<string, string>();

        public UrlMapper(Configuration cfg)
        {
            _outputDirectory = cfg.DestinationFolder;
        }

        public string GetPath(Item item)
        {
            var normalizedUrl = UrlHelper.NormalizeUrl(item.Path);
            if (_map.ContainsKey(normalizedUrl))
                return _map[normalizedUrl];

            var uri = new Uri(normalizedUrl);
            var fileName = Path.GetFileName(uri.LocalPath);
            //subUrl it is a part of url between host name and last segment (if last segment is file name)
            var subUrl = Path.GetDirectoryName(uri.LocalPath);
            //if subUrl is empty, it won't insert into link
            if (subUrl == null || subUrl == "\\")
                subUrl = string.Empty;

            //we need path to host directory for all links
            var hostUrl = UrlHelper.ExtractRoot(normalizedUrl);
            if (!_map.ContainsKey(hostUrl))
            {
                var hostPath = $"{_outputDirectory}\\{uri.Host}\\{GetFileNameOrDefault(fileName)}";
                _map.Add(hostUrl, hostPath);

                if (hostUrl == normalizedUrl)
                    return hostPath;
            }

            var hostDirectoryPath = Path.GetDirectoryName(_map[hostUrl]);
            var filePath = $"{hostDirectoryPath}{subUrl}\\{GetFileNameOrDefault(fileName)}";

            _map.Add(normalizedUrl, filePath);
            return filePath;
        }

        public string GetProcessedPathByUrl(string url)
        {
            var normalizedUrl = UrlHelper.NormalizeUrl(url);
            if (_map.ContainsKey(normalizedUrl))
                return _map[normalizedUrl];
            
            throw new InvalidOperationException($"Path {url} not processed yet.");
        }

        private static string GetFileNameOrDefault(string fileName)
        {
            return string.IsNullOrEmpty(fileName) ? "index.html" : fileName;
        }
    }
}
