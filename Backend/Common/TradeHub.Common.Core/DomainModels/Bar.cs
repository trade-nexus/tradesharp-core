

using System;

namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// Open-High-Low-Close Bars / Candles
    /// </summary>
    [Serializable()]
    public class Bar : MarketDataEvent
    {
        private string _requestId = string.Empty;
        private decimal _close = default(decimal);
        private decimal _open = default(decimal);
        private decimal _high = default(decimal);
        private decimal _low = default(decimal);
        private long _volume = default(long);
        private bool _isBarCopied = false;

        public decimal Close
        {
            get
            {
                return _close;
            }
            set
            {
                _close = value;
            }
        }

        public decimal Open
        {
            get
            {
                return _open;
            }
            set
            {
                _open = value;
            }
        }

        public decimal High
        {
            get
            {
                return _high;
            }
            set
            {
                _high = value;
            }
        }

        public decimal Low
        {
            get
            {
                return _low;
            }
            set
            {
                _low = value;
            }
        }

        public long Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = value;
            }
        }

        public string RequestId
        {
            get { return _requestId; }
            set { _requestId = value; }
        }

        /// <summary>
        /// Is bar copied from last bar values or new bar. True if copied from last bar.
        /// </summary>
        public bool IsBarCopied
        {
            get { return _isBarCopied; }
            set { _isBarCopied = value; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        private Bar() : base()
        {
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="requestId">ID which was used to request the Bar data</param>
        public Bar(string requestId) : base()
        {
            _requestId = requestId;
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="security">TradeHub Security</param>
        /// <param name="marketDataProvider">Name of Market Data provider</param>
        /// <param name="requestId">ID which was used to request the Bar data</param>
        public Bar(Security security, string marketDataProvider, string requestId)
            : base(security, marketDataProvider)
        {
            _requestId = requestId;
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="security">TradeHub Security</param>
        /// <param name="marketDataProvider">Name of Market Data provider</param>
        /// <param name="requestId">ID which was used to request the Bar data</param>
        /// <param name="dateTime">DataTime</param>
        public Bar(Security security, string marketDataProvider, string requestId, DateTime dateTime)
            : base(security, marketDataProvider, dateTime)
        {
            _requestId = requestId;
        }

        /// <summary>
        /// Overrides ToString Method
        /// </summary>
        public override String ToString()
        {
            return " Bar :: " +
                   " Market Data Provider : " + MarketDataProvider +
                   " Timestamp : " + this.DateTime.ToString("yyyyMMdd HH:mm:ss.fff") +
                   " Request ID : " + _requestId +
                   " | Open : " + this._open +
                   " | High : " + this._high +
                   " | Low : " + this._low +
                   " | Close : " + this._close +
                   " | Volume : " + this._volume +
                   " | " + Security;
        }

        /// <summary>
        /// Creates a string which is to be published and converted back to Bar on receiver end
        /// </summary>
        public String DataToPublish()
        {
            return "BAR" +
                   "," + _close +
                   "," + _open +
                   "," + _high +
                   "," + _low +
                   "," + _volume +
                   "," + Security.Symbol +
                   "," + DateTime.ToString("M/d/yyyy h:mm:ss tt") +
                   "," + MarketDataProvider +
                   "," + _requestId +
                   "," + IsBarCopied;
        }
    }
}
