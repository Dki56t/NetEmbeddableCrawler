namespace Crawler.Projections
{
    public enum TraversalMode
    {
        /// <summary>
        /// All urls remain unchanged. Content will be loaded and stored as a snapshot.
        /// </summary>
        SameHostSnapshot,

        /// <summary>
        /// The crawler will try to load content from urls with the same host as in root url until specified depth would reached.
        /// Other urls will remain unchanged,
        /// </summary>
        SameHost,

        /// <summary>
        /// The crawler will try to load content from all urls until specified depth would reached.
        /// </summary>
        AnyHost
    }
}