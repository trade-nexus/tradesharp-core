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


﻿

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
