/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* TradeSharp is a C# based data feed and broker neutral Algorithmic 
* Trading Platform that lets trading firms or individuals automate 
* any rules based trading strategies in stocks, forex and ETFs. 
* TradeSharp allows users to connect to providers like Tradier Brokerage, 
* IQFeed, FXCM, Blackwood, Forexware, Integral, HotSpot, Currenex, 
* Interactive Brokers and more. 
* Key features: Place and Manage Orders, Risk Management, 
* Generate Customized Reports etc 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.MarketDataEngine.BarFactory.Interfaces;
using TradeHubBarPriceType = TradeHub.Common.Core.Constants.BarPriceType;

namespace TradeHub.MarketDataEngine.BarFactory.Service
{
    /// <summary>
    /// Provides Displacement Bars
    /// </summary>
    internal class DisplacementBarGenerator : IEngineeredBarGenerator
    {
        private Type _type = typeof(DisplacementBarGenerator);

        private decimal _open, _close, _low, _high;

        /// <summary>
        /// Fired each time when bar is created
        /// </summary>
        public event Action<Bar, string> BarArrived;

        private readonly Security _security = null;

        private readonly Object _lockObject = new Object();

        private readonly decimal _pipSize;
        private readonly decimal _numberOfPips;

        public string BarPriceType { get; set; }
        public string BarGeneratorKey { get; set; }

        /// <summary>
        /// Argument constructor
        /// </summary>
        /// <param name="security">Symbol of bars</param>
        /// <param name="barGeneratorKey"> </param>
        /// <param name="pipSize">Minimum change in price</param>
        /// <param name="numberOfPips">Bar size in number of pips</param>
        /// <param name="barPriceType">Price Type used for Bar </param>
        public DisplacementBarGenerator(Security security, string barGeneratorKey, decimal pipSize, decimal numberOfPips, string barPriceType)
        {
            _security = security;
            _pipSize = pipSize;
            _numberOfPips = numberOfPips;
            _open = _close = _low = _high = 0m;

            BarGeneratorKey = barGeneratorKey;
            BarPriceType = barPriceType;
        }

        /// <summary>
        /// Argument constructor with Bar Seed Value
        /// </summary>
        /// <param name="security">Symbol of bars</param>
        /// <param name="barGeneratorKey"> </param>
        /// <param name="pipSize">Minimum change in price</param>
        /// <param name="numberOfPips">Bar size in number of pips</param>
        /// <param name="barPriceType">Price Type used for Bar </param>
        /// <param name="barSeed"> </param>
        public DisplacementBarGenerator(Security security, string barGeneratorKey, decimal pipSize, decimal numberOfPips, string barPriceType, decimal barSeed)
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
            if (this._open == 0)
            {
                this._open = value;
                this._low = value;
                this._high = value;
            }

            this._close = value;
            if (this._low > value)
            {
                this._low = value;
            }

            if (this._high < value)
            {
                this._high = value;
            }

            var differenceInPips = (Math.Abs(this._high - this._low)) / PipSize;
            if (differenceInPips >= _numberOfPips)
            {
                PostData(_open, _close, _high, _low);
                _open = _high = _low = _close = 0;
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
                Logger.Info(this._security + " - Posting new bar - " + bar, _type.FullName, "PostData");
            }

            // Post new bar.
            if (BarArrived != null)
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Number of subscribers to bar factory: " + BarArrived.GetInvocationList(), _type.FullName, "PostData");
                }
                BarArrived(bar, BarGeneratorKey);
            }
        }
    }
}
