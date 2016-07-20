using System;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.DataDownloader.Common.ConcreteImplementation
{
    public class ProviderPermission:Login
    {
        /// <summary>
        /// Set to True if user wants to write Tick or Bar to Csv
        /// </summary>
        public bool WriteCsv { get; set; }

        /// <summary>
        /// Set to True if user wants to write Tick or Bar to Bin
        /// </summary>
        public bool WriteBinary { get; set; }

        /// <summary>
        /// Set to True if user wants to write Tick ot Bar to DataBase
        /// </summary>
        public bool WriteDatabase { get; set; }
        
        /// <summary>
        /// Convert To String Methord
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return " | WriteDatabase : " + WriteDatabase +
                   " | WriteBinary : " + WriteBinary +
                   " | MarketDataProvider : " + MarketDataProvider +
                   " | WriteCsv : " + WriteCsv;
        }
    }
}
