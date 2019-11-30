using System;
using System.Collections.Generic;

namespace Crawler.Projections
{
    /// <summary>
    ///     Represents a result of parsing process.
    /// </summary>
    public class ProcessingResult
    {
        public ProcessingResult(IDictionary<Uri, Exception> failedUris)
        {
            FailedUris = failedUris ?? throw new ArgumentNullException(nameof(failedUris));
        }

        /// <summary>
        ///     A list of uris loading of content from which was failed.
        ///     A value for every uri is an exception occured.
        /// </summary>
        public IDictionary<Uri, Exception> FailedUris { get; }
    }
}