using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Utility
{
    /// <summary>
    /// Provides Unique Id's for market data requests
    /// </summary>
    public class MarketDataIdGenerator : IMarketDataIdGenerator
    {
        private int _tickIdCount = 0;
        private int _barIdCount = 0;
        private int _historicalDataIdCount = 0;

        /// <summary>
        /// Gets next unique Tick ID for the session
        /// </summary>
        /// <returns>string value to be used as Tick request ID</returns>
        public string NextTickId()
        {
            // Create Time part for the ID
            string idTimePart = DateTime.Now.ToString("yyMMddHmsfff");

            // Reutrn combination of Time and Count as Tick ID
            return idTimePart + Interlocked.Increment(ref _tickIdCount);
        }

        /// <summary>
        /// Gets next unique Bar ID for the session
        /// </summary>
        /// <returns>string value to be used as Bar request ID</returns>
        public string NextBarId()
        {
            // Create Time part for the ID
            string idTimePart = DateTime.Now.ToString("yyMMddHmsfff");

            // Reutrn combination of Time and Count as Bar ID
            return idTimePart + Interlocked.Increment(ref _barIdCount);
        }

        /// <summary>
        /// Gets next unique Historical Data ID for the session
        /// </summary>
        /// <returns>string value to be used as Historica Data request ID</returns>
        public string NextHistoricalDataId()
        {
            //// Create Time part for the ID
            //string idTimePart = DateTime.Now.ToString("yyMMddHmsfff");

            // Reutrn combination of Time and Count as Historical Data ID
            return Interlocked.Increment(ref _historicalDataIdCount).ToString();
        }
    }
}
