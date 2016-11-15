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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Utility
{
    /// <summary>
    /// Provides Unique Id's for market data requests
    /// </summary>
    public class MarketDataIdGenerator : IMarketDataIdGenerator
    {
        private int _tickIdCount = 0;
        private int _barIdCount = 0;
        private int _historicalDataIdCount = 0;

        /// <summary>
        /// Gets next unique Tick ID for the session
        /// </summary>
        /// <returns>string value to be used as Tick request ID</returns>
        public string NextTickId()
        {
            // Create Time part for the ID
            string idTimePart = DateTime.Now.ToString("yyMMddHmsfff");

            // Reutrn combination of Time and Count as Tick ID
            return idTimePart + Interlocked.Increment(ref _tickIdCount);
        }

        /// <summary>
        /// Gets next unique Bar ID for the session
        /// </summary>
        /// <returns>string value to be used as Bar request ID</returns>
        public string NextBarId()
        {
            // Create Time part for the ID
            string idTimePart = DateTime.Now.ToString("yyMMddHmsfff");

            // Reutrn combination of Time and Count as Bar ID
            return idTimePart + Interlocked.Increment(ref _barIdCount);
        }

        /// <summary>
        /// Gets next unique Historical Data ID for the session
        /// </summary>
        /// <returns>string value to be used as Historica Data request ID</returns>
        public string NextHistoricalDataId()
        {
            //// Create Time part for the ID
            //string idTimePart = DateTime.Now.ToString("yyMMddHmsfff");

            // Reutrn combination of Time and Count as Historical Data ID
            return Interlocked.Increment(ref _historicalDataIdCount).ToString();
        }
    }
}
