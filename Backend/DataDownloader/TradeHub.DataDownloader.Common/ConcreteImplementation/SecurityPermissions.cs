using System;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.DataDownloader.Common.ConcreteImplementation
{
    public class SecurityPermissions:Subscribe 
    {
        /// <summary>
        /// Write Trade To Medium (Csv,Binary,SQL)
        /// </summary>
        public bool WriteTrade { get; set; }

        /// <summary>
        /// Write Quote To Medium (Csv,Binary,SQL)
        /// </summary>
        public bool WriteQuote { get; set; }

        /// <summary>
        /// Write Bars to Medium (Csv,Binary,SQL)
        /// </summary>
        public bool WriteBars { get; set; }

        /// <summary>
        /// Convert To String Methord
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return " | Symbol : " + Security.Symbol +
                   " | MarketDataProvider : " + MarketDataProvider +
                   " | ID : " + Id +
                   " | WriteBars : " + WriteBars +
                   " | WriteQuote : " + WriteQuote +
                   " | WriteTrade : " + WriteTrade;
        }
    }
}
