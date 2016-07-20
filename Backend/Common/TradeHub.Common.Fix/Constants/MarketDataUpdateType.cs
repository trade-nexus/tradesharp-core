using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Fix.Constants
{
    public static class MarketDataUpdateType
    {
        /// <summary>
        /// Full refresh
        /// </summary>
        public const int FullRefresh = 0;

        /// <summary>
        /// Incremental Refresh
        /// </summary>
        public const int IncrementalRefresh = 1;
    }
}
