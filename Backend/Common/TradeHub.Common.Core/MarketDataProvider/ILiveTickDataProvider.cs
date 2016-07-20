using System;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.Common.Core.MarketDataProvider
{
    /// <summary>
    /// Interface to be implemented by Market Data Provider Gateways providing Live Market Data
    /// </summary>
    public interface ILiveTickDataProvider : IMarketDataProvider
    {
        #region Methods

        /// <summary>
        /// Market data request message
        /// </summary>
        bool SubscribeTickData(Subscribe request);

        /// <summary>
        /// Unsubscribe Market data message
        /// </summary>
        bool UnsubscribeTickData(Unsubscribe request);

        #endregion

        #region Events

        /// <summary>
        /// Fired each time a new tick arrives.
        /// </summary>
        event Action<Tick> TickArrived;

        #endregion
    }
}
