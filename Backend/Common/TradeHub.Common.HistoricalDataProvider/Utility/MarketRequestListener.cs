using System;
using TraceSourceLogger;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.Common.HistoricalDataProvider.Service;

namespace TradeHub.Common.HistoricalDataProvider.Utility
{
    /// <summary>
    /// Listens to incoming market data requests and takes appropriate actions
    /// </summary>
    public class MarketRequestListener
    {
        private Type _type = typeof(MarketRequestListener);
        private readonly AsyncClassLogger _asyncClassLogger;

        private readonly DataHandler _dataHandler;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="dataHandler">Provides requested data</param>
        /// <param name="asyncClassLogger">TraceSource Async Class Logger</param>
        public MarketRequestListener(DataHandler dataHandler, AsyncClassLogger asyncClassLogger)
        {
            // Save Instances
            _dataHandler = dataHandler;
            _asyncClassLogger = asyncClassLogger;
        }

        #region Incoming Market Data Requests

        /// <summary>
        /// Manages new incoming Tick Data request
        /// </summary>
        /// <param name="subscribe">Contains info for Tick data to be subscribed</param>
        public void SubscribeTickData(Subscribe subscribe)
        {
            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug(
                    "Sending tick subscription request to data handler for: " + subscribe.Security.Symbol,
                    _type.FullName, "SubscribeTickData");
            }

            // Send request to DataHandler
            _dataHandler.SubscribeSymbol(subscribe);
        }

        /// <summary>
        /// Manages new incoming Live Bar request
        /// </summary>
        /// <param name="barDataRequest">Contains info for the Live Bar to be subscribed</param>
        public void SubscribeLiveBars(BarDataRequest barDataRequest)
        {
            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug(
                    "Sending live bar subscription request to data handler for: " + barDataRequest.Security.Symbol,
                    _type.FullName, "SubscribeLiveBars");
            }

            // Send request to DataHandler
            _dataHandler.SubscribeSymbol(barDataRequest);
        }

        /// <summary>
        /// Manages new incoming Live Bar request
        /// </summary>
        /// <param name="barDataRequest">Contains info for the Live Bar to be subscribed</param>
        public void SubscribeMultipleLiveBars(BarDataRequest[] barDataRequest)
        {
            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug(
                    "Sending live bar subscription request to data handler for: " + barDataRequest.Length,
                    _type.FullName, "SubscribeLiveBars");
            }

            // Send request to DataHandler
            _dataHandler.SubscribeMultiSymbol(barDataRequest);
        }

        /// <summary>
        /// Manages new incoming Historical Data request
        /// </summary>
        /// <param name="historicDataRequest">Contains info for the Historical Bar data to be provided</param>
        public void SubscribeHistoricalData(HistoricDataRequest historicDataRequest)
        {
            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug(
                    "Sending historical data subscription request to data handler for: " + historicDataRequest.Security.Symbol,
                    _type.FullName, "SubscribeHistoricalData");
            }

            // Send request to DataHandler
            _dataHandler.SubscribeSymbol(historicDataRequest);
        }

        #endregion

        #region Incoming Market Data Unsubscription Requests

        /// <summary>
        /// Manages incoming unsubcription request for the subscribed Tick Data
        /// </summary>
        /// <param name="unsubscribe">Contains info for the Tick data to be unsubscribed</param>
        public void UnsubscribeTickData(Unsubscribe unsubscribe)
        {
            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug(
                    "Tick un-subscription request received for: " + unsubscribe.Security.Symbol,
                    _type.FullName, "UnsubscribeTickData");
            }

            _dataHandler.UnsubscribleSymbol(unsubscribe);
        }

        /// <summary>
        /// Manages incoming unsubcription request for the subscribed Live Bars
        /// </summary>
        /// <param name="barDataRequest">Contains info for the Live Bars to be unsubscribed</param>
        public void UnsubcribeLiveBars(BarDataRequest barDataRequest)
        {
            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug(
                    "Live Bar un-subscription request received for: " + barDataRequest.Security.Symbol,
                    _type.FullName, "UnsubcribeLiveBars");
            }

            _dataHandler.UnsubscribleSymbol(barDataRequest);
        }

        #endregion
    }
}
