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

namespace TradeHub.OrderExecutionProvider.Tradier.ValueObject
{
    /// <summary>
    /// Tradier order representation
    /// </summary>
    public class OrderFields
    {
        public int id { get; set; }
        public string type { get; set; }
        public string symbol { get; set; }
        public string side { get; set; }
        public int quantity { get; set; }
        public string status { get; set; }
        public string duration { get; set; }
        public double price { get; set; }
        public int avg_fill_price { get; set; }
        public int exec_quantity { get; set; }
        public decimal last_fill_price { get; set; }
        public int last_fill_quantity { get; set; }
        public int remaining_quantity { get; set; }
        public string create_date { get; set; }
        public string transaction_date { get; set; }
        public string @class { get; set; }
        public int num_legs { get; set; }
        public string strategy { get; set; }
        public string option_symbol { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder=new StringBuilder();
            stringBuilder.Append("ID=" + id);
            stringBuilder.Append(", type=" + type);
            stringBuilder.Append(", symbol=" + symbol);
            stringBuilder.Append(", side=" + side);
            stringBuilder.Append(", quantity=" + quantity);
            stringBuilder.Append(", status=" + status);
            stringBuilder.Append(", price=" + price);
            stringBuilder.Append(", avg_fill_price=" + avg_fill_price);
            stringBuilder.Append(", exec_quantity=" + exec_quantity);
            stringBuilder.Append(", last_fill_price=" + last_fill_price);
            stringBuilder.Append(", last_fill_quantity=" + last_fill_quantity);
            stringBuilder.Append(", remaining_quantity=" + remaining_quantity);
            stringBuilder.Append(", create_date=" + create_date);
            stringBuilder.Append(", transaction_date=" + transaction_date);
            stringBuilder.Append(", class=" + @class);
            stringBuilder.Append(", num_legs=" + num_legs);
            stringBuilder.Append(", strategy=" + strategy);
            stringBuilder.Append(", option_symbol=" + option_symbol);
            return stringBuilder.ToString();
        }
    }
}
