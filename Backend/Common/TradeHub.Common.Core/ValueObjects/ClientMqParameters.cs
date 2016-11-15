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

namespace TradeHub.Common.Core.ValueObjects
{
    /// <summary>
    /// Contains Client MQ info 
    /// </summary>
    public class ClientMqParameters
    {
        private string _appId = string.Empty;
        private string _replyTo = string.Empty;
        private string _consumerTag = string.Empty;
        private ulong _deliverTag = default(ulong);
        private string _exchangeName = string.Empty;
        private string _routingKey = string.Empty;

        /// <summary>
        /// Gets/Sets App ID
        /// </summary>
        public string AppId
        {
            get { return _appId; }
            set { _appId = value; }
        }

        /// <summary>
        /// Gets/Sets Consumer Tag
        /// </summary>
        public string ConsumerTag
        {
            get { return _consumerTag; }
            set { _consumerTag = value; }
        }

        /// <summary>
        /// Gets/Sets Deliver Tag
        /// </summary>
        public ulong DeliverTag
        {
            get { return _deliverTag; }
            set { _deliverTag = value; }
        }

        /// <summary>
        /// Gets/Sets ExchangeName
        /// </summary>
        public string ExchangeName
        {
            get { return _exchangeName; }
            set { _exchangeName = value; }
        }

        /// <summary>
        /// Gets/Sets RoutingKey
        /// </summary>
        public string RoutingKey
        {
            get { return _routingKey; }
            set { _routingKey = value; }
        }

        /// <summary>
        /// Name of the Queue on which to Reply
        /// </summary>
        public string ReplyTo
        {
            get { return _replyTo; }
            set { _replyTo = value; }
        }

        /// <summary>
        /// Overrides ToString Method to provide Client MQ Parameters Info
        /// </summary>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("ClientMqParameters :: ");
            stringBuilder.Append(" | App ID: " + AppId);
            stringBuilder.Append(" | Reply To: " + ReplyTo);
            stringBuilder.Append(" | Consumer Tag: " + ConsumerTag);
            stringBuilder.Append(" | Deliver Tag: " + DeliverTag);
            stringBuilder.Append(" | Exchange Name: " + ExchangeName);
            stringBuilder.Append(" | Routing Key: " + RoutingKey);
            return stringBuilder.ToString();
        }
    }
}
