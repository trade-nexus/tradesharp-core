using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Fix.Constants
{
    /// <summary>
    /// Order side
    /// </summary>
    public static class FixOrderSide
    {
        /// <summary>
        /// Buy Order
        /// </summary>
        public const char Buy = '1';

        /// <summary>
        /// Sell Order
        /// </summary>
        public const char Sell = '2';

        /// <summary>
        /// Buy Minus Order ?
        /// </summary>
        public const char BuyMinus = '3';

        /// <summary>
        /// Sell Plus Order ?
        /// </summary>
        public const char SellPlus = '4';

        /// <summary>
        /// Sell Short Order ?
        /// </summary>
        public const char SellShort = '5';

        /// <summary>
        /// Sell Short Exempt Order ?
        /// </summary>
        public const char SellShortExempt = '6';

        /// <summary>
        /// Undisclosed Order ?
        /// </summary>
        public const char Undisclosed = '7';

        /// <summary>
        /// Cross Order ?
        /// </summary>
        public const char Cross = '8';

        /// <summary>
        /// Cross Short Order ?
        /// </summary>
        public const char CrossShort = '9';
    }
}
