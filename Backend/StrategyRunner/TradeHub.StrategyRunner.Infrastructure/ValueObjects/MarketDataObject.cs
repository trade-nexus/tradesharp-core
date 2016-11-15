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


﻿using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    public class MarketDataObject
    {
        /// <summary>
        /// Indicated whether the object contains valid Tick or Bar
        /// </summary>
        private bool _isTick = false;

        /// <summary>
        /// TradeHub Tick object
        /// </summary>
        private Tick _tick = new Tick(new Security(), MarketDataProvider.SimulatedExchange);

        /// <summary>
        /// TradeHub Bar objecct
        /// </summary>
        private Bar _bar = new Bar(new Security(), MarketDataProvider.SimulatedExchange, "");

        /// <summary>
        /// Indicated whether the object contains valid Tick or Bar
        /// </summary>
        public bool IsTick
        {
            get { return _isTick; }
            set { _isTick = value; }
        }

        /// <summary>
        /// TradeHub Tick object
        /// </summary>
        public Tick Tick
        {
            get { return _tick; }
            set { _tick = value; }
        }

        /// <summary>
        /// TradeHub Bar objecct
        /// </summary>
        public Bar Bar
        {
            get { return _bar; }
            set { _bar = value; }
        }
    }
}
