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

namespace TradeHub.MarketDataProvider.Tradier.ValueObject
{
    /// <summary>
    /// Contains individual JSON elements received through streaming quotes
    /// </summary>
    public class QuoteStream
    {
        public string type { get; set; }
        public string symbol { get; set; }
        public string exch { get; set; }
        public string price { get; set; }
        public string size { get; set; }
        public string cvol { get; set; }
        public string date { get; set; }
        public string bid { get; set; }
        public string bidsz { get; set; }
        public string bidexch { get; set; }
        public string biddate { get; set; }
        public string ask { get; set; }
        public string asksz { get; set; }
        public string askexch { get; set; }
        public string askdate { get; set; }
        public string open { get; set; }
        public string high { get; set; }
        public string low { get; set; }
        public string prevClose { get; set; }
    }
}
