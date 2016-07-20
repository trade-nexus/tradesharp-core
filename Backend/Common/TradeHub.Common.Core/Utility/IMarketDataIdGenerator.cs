using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Utility
{
    /// <summary>
    /// Blue print Market Data ID generator
    /// </summary>
    public interface IMarketDataIdGenerator
    {
        /// <summary>
        /// Gets next unique Tick ID for the session
        /// </summary>
        /// <returns>string value to be used as Tick request ID</returns>
        string NextTickId();

        /// <summary>
        /// Gets next unique Bar ID for the session
        /// </summary>
        /// <returns>string value to be used as Bar request ID</returns>
        string NextBarId();

        /// <summary>
        /// Gets next unique Historical Data ID for the session
        /// </summary>
        /// <returns>string value to be used as Historica Data request ID</returns>
        string NextHistoricalDataId();
    }
}
