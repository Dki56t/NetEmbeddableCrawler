using Crawler.Projections;

namespace Crawler
{
    public class Configuration
    {
        /// <summary>
        /// A depth of traversal.
        /// Means hopes from root url to last loaded leaf.
        /// </summary>
        public short Depth { get; set; }

        /// <summary>
        /// A starting position for traversal and loading of the site graph.
        /// </summary>
        public string RootLink { get; set; }

        /// <summary>
        /// A mode of traversal process.
        /// </summary>
        public TraversalMode Mode { get; set; }

        /// <summary>
        /// A root directory in the local filesystem for loaded content.
        /// Sub directories can be created there to map different domains.
        /// Html responses will be saved as files with .html extension.
        /// </summary>
        public string DestinationFolder { get; set; }
    }
}