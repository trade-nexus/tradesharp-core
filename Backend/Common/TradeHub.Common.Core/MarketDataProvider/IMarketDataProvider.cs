using System;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.Common.Core.MarketDataProvider
{
    /// <summary>
    /// Interface to be implemented by Market Data Provider Gateways
    /// </summary>
    public interface IMarketDataProvider
    {
        #region Methods
        /// <summary>
        /// Connects/Starts a client
        /// </summary>
        bool Start();

        /// <summary>
        /// Disconnects/Stops a client
        /// </summary>
        bool Stop();

        /// <summary>
        /// Is Market Data client connected
        /// </summary>
        /// <returns></returns>
        bool IsConnected();

        #endregion

        #region Events
        /// <summary>
        /// Fired each time a Logon is arrived
        /// </summary>
        event Action<string> LogonArrived;

        /// <summary>
        /// Fired each time a Logout is arrived
        /// </summary>
        event Action<string> LogoutArrived;
        
        /// <summary>
        /// Fired each time when market data rejection arrives.
        /// </summary>
        event Action<MarketDataEvent> MarketDataRejectionArrived;

        #endregion
    }
}
