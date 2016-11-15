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

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    /// <summary>
    /// Represents any type of Order Execution related to a particular Security.
    /// </summary>
    [Serializable]
    public class OrderEvent
    {
        private DateTime _dateTime;
        private Security _security;
        private string _orderId;
        private string _orderExecutionProvider;

        /// <summary>
        /// Gets/Sets the Security
        /// </summary>
        public Security Security
        {
            get { return _security; }
            set { _security = value; }
        }

        /// <summary>
        /// Gets/Sets the Data Event Time
        /// </summary>
        public DateTime DateTime
        {
            get { return _dateTime; }
            set { _dateTime = value; }
        }

        /// <summary>
        /// Gets/Sets name of the Market Data Provider
        /// </summary>
        public string OrderExecutionProvider
        {
            get { return _orderExecutionProvider; }
            set { _orderExecutionProvider = value; }
        }

        /// <summary>
        /// Gets/Sets Order ID (Must be Unique)
        /// </summary>
        public string OrderId
        {
            get { return _orderId; }
            set { _orderId = value; }
        }

        public OrderEvent()
        {
            
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public OrderEvent(Security security, string orderExecutionProvider)
        {
            _security = security;
            _dateTime = DateTime.Now;
            _orderExecutionProvider = orderExecutionProvider;
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public OrderEvent(Security security, string orderExecutionProvider, DateTime dateTime)
        {
            _security = security;
            _orderExecutionProvider = orderExecutionProvider;
            _dateTime = dateTime;
        }
    }
}
