using System;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.Common.Core.MarketDataProvider
{
    /// <summary>
    /// Interface to be implemented by Market Data Provider Gateways providing Historic Tick Data
    /// </summary>
    interface IHistoricTickDataProvider : IMarketDataProvider
    {
        #region Events

        /// <summary>
        /// Fired when requested Historic Tick data arrives
        /// </summary>
        event Action<Tick[]> HistoricTickDataArrived;

        #endregion

        #region Methods

        /// <summary>
        /// Historic Tick Data Request Message
        /// </summary>
        bool RequestHistoricTickData(HistoricDataRequest historicDataRequest);

        #endregion

    }
}
