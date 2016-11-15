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
using ProtoBuf;

namespace TradeHub.SimulatedExchange.DomainObjects
{
    [ProtoContract]
    public class ProtobufBar
    {
        /// <summary>
        /// Close price of bar
        /// </summary>
        [ProtoMember(1)]
        public decimal Close { get; set; }

        /// <summary>
        /// High Price of Bar
        /// </summary>
        [ProtoMember(2)]
        public decimal High { get; set; }

        /// <summary>
        /// Low Price of Bar 
        /// </summary>
        [ProtoMember(3)]
        public decimal Low { get; set; }

        /// <summary>
        /// Open Price of Bar
        /// </summary>
        [ProtoMember(4)]
        public decimal Open { get; set; }

        /// <summary>
        /// Symbol of a bar
        /// </summary>
        [ProtoMember(5)]
        public string Symbol { get; set; }

        /// <summary>
        /// Name of Market Data Provider
        /// </summary>
        [ProtoMember(6)]
        public decimal MarketDataProvider { get; set; }

        /// <summary>
        /// Saves DataTime Of Bar
        /// </summary>
        [ProtoMember(7)]
        public DateTime DateTime { get; set; }
    }

}
