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

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    [Serializable]
    public class Rejection : OrderEvent, ICloneable
    {
        private string _rejectioReason;

        private Rejection() : base(new Security(), "")
        {
            
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public Rejection(Security security, string orderExecutionProvider) : base(security, orderExecutionProvider)
        {
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public Rejection(Security security, string orderExecutionProvider, DateTime dateTime) : base(security, orderExecutionProvider, dateTime)
        {
        }

        /// <summary>
        /// Gets/Sets Order Rejection Reason
        /// </summary>
        public string RejectioReason
        {
            get { return _rejectioReason; }
            set { _rejectioReason = value; }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            return " Rejection :: " +
                   Security +
                   " | Order Execution Provider : " + OrderExecutionProvider +
                   " | Order ID : " + OrderId +
                   " | Rejection Reason : " + RejectioReason;
        }
    }
}
