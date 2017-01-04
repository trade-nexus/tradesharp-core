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


using System;
using TradeHub.Common.Core.Constants;

namespace TradeSharp.UI.Common.ValueObjects
{
    /// <summary>
    /// Contains details for the Historical Bars
    /// </summary>
    public class HistoricalBarParameters
    {
        /// <summary>
        /// Type of Historical Bar e.g. Tick, Trade, Daily, Intra Day, etc.
        /// </summary>
        private string _type;

        /// <summary>
        /// Starting date from which to fetch the historical bar data
        /// </summary>
        private DateTime _startDate;

        /// <summary>
        /// End date for the range of historical bar data
        /// </summary>
        private DateTime _endDate;

        /// <summary>
        /// Bar interval for historical data
        /// </summary>
        private uint _interval;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public HistoricalBarParameters()
        {
            _interval = 60;
            _type = BarType.DAILY;
            _startDate = DateTime.UtcNow;
            _endDate = DateTime.UtcNow;
        }


        /// <summary>
        /// Type of Historical Bar e.g. Tick, Trade, Daily, Intra Day, etc.
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        /// Starting date from which to fetch the historical bar data
        /// </summary>
        public DateTime StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        /// <summary>
        /// End date for the range of historical bar data
        /// </summary>
        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        /// <summary>
        /// Bar interval for historical data
        /// </summary>
        public uint Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }
    }
}
