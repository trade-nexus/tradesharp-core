using System;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.MarketDataEngine.BarFactory.Interfaces;
using TradeHubBarPriceType = TradeHub.Common.Core.Constants.BarPriceType;

namespace TradeHub.MarketDataEngine.BarFactory.Service
{
    /// <summary>
    /// Generates Engineered Unequal Bars from the incoming Ticks
    /// </summary>
    internal class EngineeredUnequalBarGenerator :IEngineeredBarGenerator
    {
        private Type _type = typeof(EngineeredUnequalBarGenerator);

        private decimal _open, _close, _low, _high;

        /// <summary>
        /// Fired each time when bar is created
        /// </summary>
        public event Action<Bar, string> BarArrived;

        private readonly Security _security = null;
        private Decimal? _baseValue = null;

        private readonly Object _lockObject = new Object();

        private readonly decimal _pipSize;
        private readonly decimal _numberOfPips;

        public string BarPriceType { get; set; }
        public string BarGeneratorKey { get; set; }

        /// <summary>
        /// Argument constructor
        /// </summary>
        /// <param name="security">TradeHub Security</param>
        /// <param name="barGeneratorKey"> </param>
        /// <param name="pipSize">Minimum change in price</param>
        /// <param name="numberOfPips">Bar size in number of pips</param>
        /// <param name="barPriceType"> </param>
        public EngineeredUnequalBarGenerator(Security security, string barGeneratorKey, decimal pipSize, decimal numberOfPips, string barPriceType)
        {
            _security = security;
            _pipSize = pipSize;
            _numberOfPips = numberOfPips;
            _open = _close = _low = _high = 0m;
            BarGeneratorKey = barGeneratorKey;
            BarPriceType = barPriceType;
        }

        /// <summary>
        /// Argument constructor with Bar Seed value
        /// </summary>
        /// <param name="security">TradeHub Security</param>
        /// <param name="barGeneratorKey"> </param>
        /// <param name="pipSize">Minimum change in price</param>
        /// <param name="numberOfPips">Bar size in number of pips</param>
        /// <param name="barPriceType"> </param>
        /// <param name="barSeed"> </param>
        public EngineeredUnequalBarGenerator(Security security, string barGeneratorKey, decimal pipSize, decimal numberOfPips, string barPriceType, decimal barSeed)
        {
            _security = security;
            _pipSize = pipSize;
            _numberOfPips = numberOfPips;
            _open = _close = _low = _high = barSeed;

            BarGeneratorKey = barGeneratorKey;
            BarPriceType = barPriceType;
        }

        /// <summary>
        /// Update OHLC values
        /// </summary>
        /// <param name="tick"></param>
        public void Update(Tick tick)
        {
            if (tick == null)
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(this._security + " - The tick is null.", _type.FullName, "Update");
                }
                return;
            }

            if (!this._security.Equals(tick.Security.Symbol))
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(this._security + " - Symbols don't match.", _type.FullName, "Update");
                }
            }

            lock (this._lockObject)
            {
                decimal price = tick.LastPrice;
                if (this.BarPriceType == TradeHubBarPriceType.ASK)
                    price = tick.AskPrice;
                else if (this.BarPriceType == TradeHubBarPriceType.BID)
                    price = tick.BidPrice;
                ApplyValue(price);
            }
        }

        /// <summary>
        /// Apply OHLC values
        /// </summary>
        /// <param name="value"></param>
        private void ApplyValue(decimal value)
        {
            if (_baseValue == null)
            {
                _baseValue = _open = _close = _low = _high = value;
            }
            else
            {
                var difference = Math.Abs(_baseValue.Value - value);
                if (difference > 0)
                {
                    var differenceInPips = difference * (1 / _pipSize);
                    if (differenceInPips >= _numberOfPips)
                    {
                        if (value > _baseValue)
                        {
                            _high = _close = value;
                        }
                        else
                        {
                            _low = _close = value;
                        }
                        PostData(_open, _close, _high, _low);
                        _baseValue = _open = _low = _high = _close; 

                    }
                }
            }

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(this._security + " - New value applied - " + value, _type.FullName, "ApplyValue");
            }
        }

        /// <summary>
        /// Get symbol
        /// </summary>
        public Security Security
        {
            get 
            {
                return _security;
            }
        }

        /// <summary>
        /// Get pip size, the minimum change in price
        /// </summary>
        public decimal PipSize
        {
            get
            {
                return this._pipSize;
            }
        }

        /// <summary>
        /// Get number of pips
        /// </summary>
        public decimal NumberOfPips
        {
            get
            {
                return this._numberOfPips;
            }
        }

        /// <summary>
        /// Post data
        /// </summary>
        private void PostData(decimal open, decimal close, decimal high, decimal low)
        {
            Bar bar = new Bar(new Security { Symbol = _security.Symbol }, "Bar Factory", "",DateTime.UtcNow)
                {
                    Open = open,
                    Close = close,
                    High = high,
                    Low = low,
                    Volume = 0
                };

            if (Logger.IsInfoEnabled)
            {
                Logger.Info(this._security + " - Posting new bar - " + bar, _type.FullName, "PostData");
            }

            // Post new bar.
            if (BarArrived != null)
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Number of subscribers to bar factory: " + BarArrived.GetInvocationList().Length, _type.FullName, "PostData");
                }
                BarArrived(bar, BarGeneratorKey);
            }
        }
    }
}
