using System;
using System.Collections.Generic;
using Crawler.Logic;

namespace Crawler.Projections
{
    public sealed class ParsingResult
    {
        public ParsingResult(IReadOnlyCollection<Item> deeperItems, WalkContext context)
        {
            DeeperItems = deeperItems ?? throw new ArgumentNullException(nameof(deeperItems));
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IReadOnlyCollection<Item> DeeperItems { get; }
        public WalkContext Context { get; }
    }
}