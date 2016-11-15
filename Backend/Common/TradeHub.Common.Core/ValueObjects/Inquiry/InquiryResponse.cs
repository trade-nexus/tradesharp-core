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
    /// Contains the information in response to the <see cref="InquiryMessage"/>
    /// </summary>
    public class InquiryResponse
    {
        private string _type = string.Empty;
        private string _marketDataProvider = string.Empty;
        private string _orderExecutionProvider = string.Empty;
        private string _appId = string.Empty;
        private List<Type> _marketDataProviderInfo = new List<Type>();
        private List<Type> _orderExecutionProviderInfo = new List<Type>();

        /// <summary>
        /// The Type of requesting Query
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        /// Name of the Market Data Provider for which the query response is intended
        /// </summary>
        public string MarketDataProvider
        {
            get { return _marketDataProvider; }
            set { _marketDataProvider = value; }
        }

        /// <summary>
        /// Name of the Order Execution Provider for which the query response is intended
        /// </summary>
        public string OrderExecutionProvider
        {
            get { return _orderExecutionProvider; }
            set { _orderExecutionProvider = value; }
        }

        /// <summary>
        /// Newly generated Unique App ID
        /// </summary>
        public string AppId
        {
            get { return _appId; }
            set { _appId = value; }
        }

        /// <summary>
        /// Contains the list of Types which the requested Market Data Provider supports
        /// </summary>
        public List<Type> MarketDataProviderInfo
        {
            get { return _marketDataProviderInfo; }
            set { _marketDataProviderInfo = value; }
        }

        /// <summary>
        /// Contains the list of Types which the requested Order Execution Provider supports
        /// </summary>
        public List<Type> OrderExecutionProviderInfo
        {
            get { return _orderExecutionProviderInfo; }
            set { _orderExecutionProviderInfo = value; }
        }
    }
}
