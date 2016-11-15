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


﻿using System.Text;
using TradeHub.Common.Core.Constants;

namespace TradeHub.Common.Core.ValueObjects.AdminMessages
{
    /// <summary>
    /// Login message to connect to the Gateway
    /// </summary>
    public class Login : IAdminMessage
    {
        // Identifies the Admin Message Type
        public  string AdminMessageType
        { 
            get { return Constants.AdminMessageType.Login; } 
        }

        // Name of Market Data Provider
        public string MarketDataProvider { get; set; }

        // Name of Order Execution Provider
        public string OrderExecutionProvider { get; set; }

        /// <summary>
        /// Overrides ToString Method to provide Login Message Info
        /// </summary>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Login :: ");
            stringBuilder.Append(" | Market Data Provider: " + MarketDataProvider);
            stringBuilder.Append(" | Order Execution Provider: " + OrderExecutionProvider);
            return stringBuilder.ToString();
        }
    }
}
