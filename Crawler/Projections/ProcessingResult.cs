using System;
using System.Collections.Generic;

namespace Crawler.Projections
{
    /// <summary>
    ///     Represents a result of parsing process.
    /// </summary>
    public class ProcessingResult
    {
        public ProcessingResult(IDictionary<string, Exception> failedUrls)
        {
            FailedUrls = failedUrls ?? throw new ArgumentNullException(nameof(failedUrls));
        }

        /// <summary>
        ///     A list of urls loading of content from which was failed.
        ///     A value for every url is an exception occured.
        /// </summary>
        public IDictionary<string, Exception> FailedUrls { get; }
    }
}