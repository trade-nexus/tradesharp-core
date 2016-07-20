using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.MarketDataEngine.BarFactory.Interfaces;
using TradeHubBarPriceType = TradeHub.Common.Core.Constants.BarPriceType;

namespace TradeHub.MarketDataEngine.BarFactory.Service
{
    /// <summary>
    /// Provides Engineered Equal Bars from the incoming Ticks
    /// </summary>
    internal class EngineeredEqualBarGenerator : IEngineeredBarGenerator
    {
        private Type _type = typeof(EngineeredEqualBarGenerator);

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
        /// <param name="barPriceType">Type of price used by Bar </param>
        public EngineeredEqualBarGenerator(Security security, string barGeneratorKey, decimal pipSize,
                                           decimal numberOfPips, string barPriceType)
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
        public EngineeredEqualBarGenerator(Security security, string barGeneratorKey, decimal pipSize,
                                           decimal numberOfPips, string barPriceType, decimal barSeed)
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
                    Logger.Debug(this._security + " - The tick is null.",
                                 _type.FullName, "Update");
                }
                return;
            }

            if (!this._security.Equals(tick.Security.Symbol))
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(this._security + " - Symbols don't match.",
                                 _type.FullName, "Update");
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
                    var differenceInPips = difference*(1/_pipSize);
                    if (differenceInPips >= _numberOfPips)
                    {
                        var numberOfBars = Convert.ToInt16(Math.Floor(differenceInPips/_numberOfPips));
                        for (var i = 0; i < numberOfBars; i++)
                        {
                            if (value > _baseValue)
                            {
                                _high = _close = _baseValue.Value + ((i + 1)*_pipSize*_numberOfPips);
                            }
                            else
                            {
                                _low = _close = _baseValue.Value - ((i + 1)*_pipSize*_numberOfPips);
                            }
                            PostData(_open, _close, _high, _low);
                            _open = _high = _low = _close;
                        }
                        _baseValue = _close;
                    }
                }
            }

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(this._security + " - New value applied - " + value,
                             _type.FullName, "ApplyValue");
            }
        }

        /// <summary>
        /// Get Security
        /// </summary>
        public Security Security
        {
            get { return _security; }
        }

        /// <summary>
        /// Get pip size, the minimum change in price
        /// </summary>
        public decimal PipSize
        {
            get { return this._pipSize; }
        }

        /// <summary>
        /// Get number of pips
        /// </summary>
        public decimal NumberOfPips
        {
            get { return this._numberOfPips; }
        }

        /// <summary>
        /// Post data
        /// </summary>
        private void PostData(decimal open, decimal close, decimal high, decimal low)
        {
            Bar bar = new Bar(new Security {Symbol = _security.Symbol}, "Bar Factory", "",DateTime.UtcNow)
                {
                    Open = open,
                    Close = close,
                    High = high,
                    Low = low,
                    Volume = 0
                };

            if (Logger.IsInfoEnabled)
            {
                Logger.Info(this._security + " - Posting new bar - " + bar,
                            _type.FullName, "PostData");
            }

            // Post new bar.
            if (BarArrived != null)
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Number of subscribers to bar factory: " + BarArrived.GetInvocationList(),
                                 _type.FullName, "PostData");
                }
                BarArrived(bar, BarGeneratorKey);
            }
        }
    }
}
