using System;
using System.IO;
using Crawler.Projections;

namespace Crawler
{
    public class Configuration
    {
        /// <summary>
        ///     Creates a crawlers' configuration
        /// </summary>
        /// <param name="rootLink">A starting position for traversal and loading of the site graph.</param>
        /// <param name="destinationDirectory">
        ///     A root directory in the local filesystem for loaded content.
        ///     Sub directories can be created there to map different domains
        ///     Html responses will be saved as files with .html extension.
        /// </param>
        public Configuration(string rootLink, string destinationDirectory)
        {
            if (string.IsNullOrEmpty(rootLink))
                throw new ArgumentNullException(nameof(rootLink));

            if (string.IsNullOrEmpty(destinationDirectory))
                throw new ArgumentNullException(nameof(rootLink));

            RootLink = rootLink;
            DestinationDirectory = destinationDirectory;

            var tmpPath = Path.GetTempPath();
            tmpPath = tmpPath.Remove(tmpPath.LastIndexOf(Path.DirectorySeparatorChar));
            DestinationDirectory = DestinationDirectory.Replace("${TempPath}", tmpPath);
        }

        /// <summary>
        ///     A depth of traversal.
        ///     Means hops from root uri to last loaded leaf.
        /// </summary>
        public short Depth { get; set; }

        /// <summary>
        ///     A starting position for traversal and loading of the site graph.
        /// </summary>
        public string RootLink { get; }

        /// <summary>
        ///     A mode of traversal process.
        /// </summary>
        public TraversalMode Mode { get; set; }

        /// <summary>
        ///     A root directory in the local filesystem for loaded content.
        ///     Sub directories can be created there to map different domains.
        ///     Html responses will be saved as files with .html extension.
        /// </summary>
        public string DestinationDirectory { get; }
    }
}