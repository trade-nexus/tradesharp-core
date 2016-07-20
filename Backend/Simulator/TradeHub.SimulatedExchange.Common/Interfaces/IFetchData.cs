using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.SimulatedExchange.Common.Interfaces
{
    /// <summary>
    /// Interface to be implemented by the class providing requested data
    /// </summary>
    public interface IFetchData
    {
        /// <summary>
        /// Reading Data From ReadMarketData class.
        /// </summary>
        /// <param name="request"></param>
        void ReadData(BarDataRequest request);

        /// <summary>
        /// Reads data for required symbol from stored files
        /// </summary>
        /// <param name="subscribe">Contains Symbol info</param>
        void ReadData(Subscribe subscribe);

        /// <summary>
        /// Reads data for required symbol from stored files
        /// </summary>
        /// <param name="historicDataRequest">Contains historical request info for subscribing symbol</param>
        void ReadData(HistoricDataRequest historicDataRequest);
    }
}
