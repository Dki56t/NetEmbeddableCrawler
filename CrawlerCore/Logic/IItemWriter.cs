﻿using System.Threading.Tasks;
using Crawler.Projections;

namespace Crawler.Logic
{
    internal interface IItemWriter
    {
        Task Write(Item item);
    }
}