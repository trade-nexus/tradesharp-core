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

namespace TradeHub.MarketDataProvider.ESignal.ValueObjects
{
    /// <summary>
    /// Contains Properties required for ESignal Connection
    /// </summary>
    public class ConnectionParameters
    {
        private string _userName;
        private string _password;

        #region Properties

        /// <summary>
        /// Gets/Sets the Password required to establish connection
        /// </summary>
        public string Password
        {
            set { _password = value; }
            get { return _password; }
        }

        /// <summary>
        /// Gets/Sets the Username used for establishing connection
        /// </summary>
        public string UserName
        {
            set { _userName = value; }
            get { return _userName; }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ConnectionParameters()
        {
            _userName = string.Empty;
            _password = string.Empty;
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public ConnectionParameters(string userName, string password)
        {
            _userName = userName;
            _password = password;
        }

        /// <summary>
        /// Overrides ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Attributes :: ");
            stringBuilder.Append(" | Username:" + _userName);
            stringBuilder.Append(" | Password:" + _password);

            return stringBuilder.ToString();
        }
    }
}
