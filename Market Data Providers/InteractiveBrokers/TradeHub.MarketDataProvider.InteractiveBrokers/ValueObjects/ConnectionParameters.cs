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

namespace TradeHub.MarketDataProvider.InteractiveBrokers.ValueObjects
{
    /// <summary>
    /// Contains Properties required for IB Connection
    /// </summary>
    public class ConnectionParameters
    {
        private string _host;
        private int _port;
        private int _clientId;

        #region Properties

        /// <summary>
        /// Gets/Sets the Host used for establishing connection
        /// </summary>
        public string Host
        {
            set { _host = value; }
            get { return _host; }
        }

        /// <summary>
        /// Gets/Sets the Port on which the Market Data is available
        /// </summary>
        public int Port
        {
            set { _port = value; }
            get { return _port; }
        }

        /// <summary>
        /// Gets/Sets Client ID
        /// </summary>
        public int ClientId
        {
            set { _clientId = value; }
            get { return _clientId; }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ConnectionParameters()
        {
            _host = string.Empty;
            _port = default(int);
            _clientId = default(int);
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public ConnectionParameters(string host, int dataPort, int clientId)
        {
            _host = host;
            _port = dataPort;
            _clientId = clientId;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Attributes :: ");
            stringBuilder.Append(" | Host:" + _host);
            stringBuilder.Append(" | Port:" + _port);
            stringBuilder.Append(" | Client ID:" + _clientId);

            return stringBuilder.ToString();
        }
    }
}
