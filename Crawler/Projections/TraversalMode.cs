namespace Crawler.Projections
{
    /// <summary>
    ///     Represents a mode of parsing process.
    /// </summary>
    public enum TraversalMode
    {
        /// <summary>
        ///     All uris remain unchanged. Content will be loaded and stored as a snapshot.
        /// </summary>
        SameHostSnapshot,

        /// <summary>
        ///     The crawler will try to load content from uris with the same host as in root uri
        ///     until specified depth would be reached.
        ///     Other uris will remain unchanged.
        /// </summary>
        SameHost,

        /// <summary>
        ///     The crawler will try to load content from all uris until specified depth would reached.
        /// </summary>
        AnyHost
    }
}