using System;

namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// Represents any type of market data related to a particular Security.
    /// </summary>
    [Serializable()]
    public class MarketDataEvent: IDisposable
    {
        private DateTime _dateTime;
        private Security _security;
        private string _marketDataProvider;

        /// <summary>
        /// Gets/Sets the Security
        /// </summary>
        public Security Security
        {
            get { return _security; }
            set { _security = value; }
        }

        /// <summary>
        /// Gets/Sets the Data Event Time
        /// </summary>
        public DateTime DateTime
        {
            get { return _dateTime; }
            set { _dateTime = value; }
        }

        /// <summary>
        /// Gets/Sets name of the Market Data Provider
        /// </summary>
        public string MarketDataProvider
        {
            get { return _marketDataProvider; }
            set { _marketDataProvider = value; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MarketDataEvent()
        {
            this._dateTime = DateTime.Now;
            _security = new Security();
            _marketDataProvider = string.Empty;
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public MarketDataEvent(Security security, string marketDataProvider)
        {
            _security = security;
            _dateTime = DateTime.Now;
            _marketDataProvider = marketDataProvider;
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public MarketDataEvent(Security security, string marketDataProvider, DateTime dateTime)
        {
            _security = security;
            _marketDataProvider = marketDataProvider;
            _dateTime = dateTime;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
