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
using System.Threading.Tasks;

namespace TradeHub.Common.Core.ValueObjects.Inquiry
{
    /// <summary>
    /// Contains information regarding which functionality is provider by Market Data Provider
    /// </summary>
    public class MarketDataProviderInfo
    {
        private string _dataProviderName = string.Empty;
        private bool _providesTickData = false;
        private bool _providesLiveBarData = false;
        private bool _providesHistoricalBarData = false;

        public bool ProvidesTickData
        {
            get { return _providesTickData; }
            set { _providesTickData = value; }
        }

        public bool ProvidesLiveBarData
        {
            get { return _providesLiveBarData; }
            set { _providesLiveBarData = value; }
        }

        public bool ProvidesHistoricalBarData
        {
            get { return _providesHistoricalBarData; }
            set { _providesHistoricalBarData = value; }
        }

        public string DataProviderName
        {
            get { return _dataProviderName; }
            set { _dataProviderName = value; }
        }

        /// <summary>
        /// ToString overrride for Market Data Provdider Info
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("MarketDataProviderInfo :: ");
            stringBuilder.Append(" Name: " + _dataProviderName);
            stringBuilder.Append(" | Ticks: " + _providesTickData);
            stringBuilder.Append(" | Live Bars: " + _providesLiveBarData);
            stringBuilder.Append(" | Historical Bars: " + _providesHistoricalBarData);

            return stringBuilder.ToString();
        }
    }
}
