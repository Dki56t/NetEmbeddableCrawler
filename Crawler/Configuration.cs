using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class Configuration
    {
        public short Depth { get; set; }

        public string RootLink { get; set; }

        public bool FullTraversal { get; set; }

        public string DestinationFolder { get; set; }
    }
}
