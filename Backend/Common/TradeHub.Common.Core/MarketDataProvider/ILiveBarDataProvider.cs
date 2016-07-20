using System;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.Common.Core.MarketDataProvider
{
    /// <summary>
    /// Interface to be implemented by Market Data Provider Gateways providing Bar Data
    /// </summary>
    public interface ILiveBarDataProvider : IMarketDataProvider
    {
        #region Events

        /// <summary>
        /// Fired each time a new Bar Arrives
        /// Bar =  TradeHub Bar Object
        /// String =  Request ID
        /// </summary>
        event Action<Bar, string> BarArrived;

        #endregion

        #region Methods

        /// <summary>
        /// Request to get Bar Data
        /// </summary>
        /// <param name="barDataRequest">TradeHub Bar Data Request Message</param>
        /// <returns></returns>
        bool SubscribeBars(BarDataRequest barDataRequest);

        /// <summary>
        /// Unsubscribe Bar data
        /// </summary>
        /// <param name="barDataRequest">TradeHub Bar Data Request Message</param>
        /// <returns></returns>
        bool UnsubscribeBars(BarDataRequest barDataRequest);
        
        #endregion

    }
}
