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
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// Contains Historical Bar Data info
    /// </summary>
    [Serializable()]
    public class HistoricBarData : MarketDataEvent
    {
        // Historical Bars
        private Bar[] _bars;

        // Request ID for the Historical Bar Data Requests
        private string _reqId;

        /// <summary>
        /// Contains detailed information of received bars
        /// </summary>
        private HistoricDataRequest _barsInformation;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public HistoricBarData(Security security, string marketDataProvider, DateTime dateTime)
            : base(security, marketDataProvider, dateTime)
        {

        }

        /// <summary>
        /// Gets/Sets Historical Bars array
        /// </summary>
        public Bar[] Bars
        {
            get { return _bars; }
            set { _bars = value; }
        }

        /// <summary>
        /// Gets/Sets Request ID for the Historical Bar Data Requests
        /// </summary>
        public string ReqId
        {
            get { return _reqId; }
            set { _reqId = value; }
        }

        /// <summary>
        /// Contains detailed information of received bars
        /// </summary>
        public HistoricDataRequest BarsInformation
        {
            get { return _barsInformation; }
            set { _barsInformation = value; }
        }

        /// <summary>
        /// Overrides ToString Method
        /// </summary>
        public override String ToString()
        {
            return " HistoricBarData :: " +
                   " Timestamp : " + DateTime +
                   " | Market Data Provider : " + MarketDataProvider +
                   " | Request ID : " + ReqId +
                   " | " + Security;
        }
    }
}
