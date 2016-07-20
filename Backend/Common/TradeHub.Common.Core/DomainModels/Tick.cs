using System;
using System.Text;

namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// Snapshot of the market at a particular point in time, containing information like
    /// last price, last time, bid, ask, volume, etc.
    /// </summary>
    [Serializable()]
    public class Tick : MarketDataEvent, ICloneable
    {
        private decimal _lastSize;
        private decimal _lastPrice;
        private string _lastExchange;
        private int _depth;
        private decimal _bidPrice;
        private decimal _bidSize;
        private string _bidExchange;
        private decimal _askPrice;
        private string _askExchange;
        private decimal _askSize;

        #region Properties

        /// <summary>
        /// Last size of tick
        /// </summary>
        public decimal LastSize
        {
            get { return this._lastSize; }
            set { this._lastSize = value; }
        }

        /// <summary>
        /// Last price of tick
        /// </summary>
        public decimal LastPrice
        {
            get { return this._lastPrice; }
            set { this._lastPrice = value; }
        }

        /// <summary>
        /// Exchange of las tick
        /// </summary>
        public string LastExchange
        {
            get { return this._lastExchange; }
            set { this._lastExchange = value; }
        }

        /// <summary>
        /// Depth in market book
        /// </summary>
        public int Depth
        {
            get { return this._depth; }
            set { this._depth = value; }
        }

        /// <summary>
        /// Bid price of tick
        /// </summary>
        public decimal BidPrice
        {
            get { return this._bidPrice; }
            set { this._bidPrice = value; }
        }

        /// <summary>
        /// Bid size of tick
        /// </summary>
        public decimal BidSize
        {
            get { return this._bidSize; }
            set { this._bidSize = value; }
        }

        /// <summary>
        /// Bid exchange of tick
        /// </summary>
        public string BidExchange
        {
            get { return this._bidExchange; }
            set { this._bidExchange = value; }
        }

        /// <summary>
        /// Ask price of tick
        /// </summary>
        public decimal AskPrice
        {
            get { return this._askPrice; }
            set { this._askPrice = value; }
        }

        /// <summary>
        /// Ask size of tick
        /// </summary>
        public decimal AskSize
        {
            get { return this._askSize; }
            set { this._askSize = value; }
        }

        /// <summary>
        /// Ask exchange of tick
        /// </summary>
        public string AskExchange
        {
            get { return this._askExchange; }
            set { this._askExchange = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Tick() : base()
        {
            InitializeFields();
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="security">TradeHub Security</param>
        /// <param name="marketDataProvider">Name of Market Data provider</param>
        public Tick(Security security, string marketDataProvider)
            : base(security, marketDataProvider)
        {
            InitializeFields();
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="security">TradeHub Security</param>
        /// <param name="marketDataProvider">Name of Market Data provider</param>
        /// <param name="dateTime">DataTime</param>
        public Tick(Security security, string marketDataProvider, DateTime dateTime)
            : base(security, marketDataProvider, dateTime)
        {
            InitializeFields();
        }

        #endregion

        /// <summary>
        /// Initializes private fields
        /// </summary>
        private void InitializeFields()
        {
            _lastSize = default(decimal);
            _lastPrice = default(decimal);
            _lastExchange = string.Empty;
            _depth = default(int);
            _bidPrice = default(decimal);
            _bidSize = default(decimal);
            _bidExchange = string.Empty;
            _askPrice = default(decimal);
            _askExchange = string.Empty;
            _askSize = default(decimal);
        }

        /// <summary>
        /// Does this tick has trade?
        /// </summary>
        public bool HasTrade
        {
            get { return (this._lastPrice != 0M) && (this._lastSize != 0M); }
        }

        /// <summary>
        /// Does this tick has bid?
        /// </summary>
        public bool HasBid
        {
            get { return (this._bidPrice != 0M) && (this._bidSize != 0M); }
        }

        /// <summary>
        /// Does this tick has ask?
        /// </summary>
        public bool HasAsk
        {
            get { return (this._askPrice != 0M) && (this._askSize != 0M); }
        }

        /// <summary>
        /// Overrides default ToString() method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var str = new StringBuilder();

            str.Append("Tick :: ");
            str.Append("Market Data Provider : " + MarketDataProvider);
            str.Append(" | " + Security);
            str.Append(" | ");
            str.Append("DateTime : " + DateTime.ToString("yyyyMMdd HH:mm:ss.fff"));

            if (HasAsk)
            {
                str.Append(" | ");
                str.Append("AskPrice : " + this._askPrice);
                str.Append(" | ");
                str.Append("AskSize : " + this._askSize);
                str.Append(" | ");
                str.Append("AskExchange : " + this._askExchange);
            }

            if (HasBid)
            {
                str.Append(" | ");
                str.Append("BidPrice : " + this._bidPrice);
                str.Append(" | ");
                str.Append("BidSize : " + this._bidSize);
                str.Append(" | ");
                str.Append("BidExchange : " + this._bidExchange);
            }

            if (HasTrade)
            {
                str.Append(" | ");
                str.Append("LastPrice : " + this._lastPrice);
                str.Append(" | ");
                str.Append("LastSize : " + this._lastSize);
                str.Append(" | ");
                str.Append("LastExchange : " + this._lastExchange);
            }

            return str.ToString();
        }

        /// <summary>
        /// Creates a string which is to be published and converted back to Tick on receiver end
        /// </summary>
        public String DataToPublish()
        {
            //var str = new StringBuilder();
            //str.Append("TICK");
            //str.Append("," + _bidPrice);
            //str.Append("," + _bidSize);
            //str.Append("," + _askPrice);
            //str.Append("," + _askSize);
            //str.Append("," + _lastPrice);
            //str.Append("," + _lastSize);
            //str.Append("," + Security.Symbol);
            //str.Append("," + DateTime.ToString("M/d/yyyy h:mm:ss.fff tt"));
            //str.Append("," + MarketDataProvider);
            //return str.ToString();
            return "TICK" +
                   "," + _bidPrice +
                   "," + _bidSize +
                   "," + _askPrice +
                   "," + _askSize +
                   "," + _lastPrice +
                   "," + _lastSize +
                   "," + Security.Symbol +
                   "," + DateTime.ToString("M/d/yyyy h:mm:ss.fff tt") +
                   "," + MarketDataProvider +
                   "," + _depth;
        }

        /// <summary>
        /// Clone Object
        /// </summary>
        /// <returns>Copy of object</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
