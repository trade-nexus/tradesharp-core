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
using System.Text;

namespace TradeHub.MarketDataProvider.Tradier.ValueObject
{
    /// <summary>
    /// Contains Properties required for Tradier Connection
    /// </summary>
    public class Credentials
    {
        private string _apiUrl;
        private string _accessToken;

        #region Properties

        /// <summary>
        /// Gets/Sets the base API Url used for establishing connection
        /// </summary>
        public string ApiUrl
        {
            set { _apiUrl = value; }
            get { return _apiUrl; }
        }

        /// <summary>
        /// Gets/Sets the Access token required for connection
        /// </summary>
        public string AccessToken
        {
            set { _accessToken = value; }
            get { return _accessToken; }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Credentials()
        {
            _apiUrl = string.Empty;
            _accessToken = string.Empty;
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public Credentials(string apiUrl, string accessToken)
        {
            _apiUrl = apiUrl;
            _accessToken = accessToken;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Attributes :: ");
            stringBuilder.Append(" | API Url:" + _apiUrl);
            stringBuilder.Append(" | Access Token:" +_accessToken);

            return stringBuilder.ToString();
        }
    }
}
