namespace Crawler
{
    public class Configuration
    {
        /// <summary>
        ///
        /// Depth restrict html pages hierarchy only, it means that non-html content
        /// can be loaded from <seealso cref="Depth"/> + 1 depth 
        /// </summary>
        public short Depth { get; set; }

        public string RootLink { get; set; }

        public bool FullTraversal { get; set; }

        public string DestinationFolder { get; set; }
    }
}