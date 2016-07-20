using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Fix.Constants
{
    /// <summary>
    /// Market data entry type
    /// </summary>
    public static class MarketDataEntryType
    {
        /// <summary>
        /// Bid
        /// </summary>
        public const char Bid = '0';

        /// <summary>
        /// Offer
        /// </summary>
        public const char Offer = '1';

        /// <summary>
        /// Trade
        /// </summary>
        public const char Trade = '2';
    }
}
