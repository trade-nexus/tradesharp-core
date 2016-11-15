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
using TraceSourceLogger;

namespace TradeHub.MarketDataProvider.Redi.Utility
{
    public static class Parser
    {
        public static string StringValue(string message)
        {
            try
            {
                string temp = message.TrimStart('|');
                int start = temp.IndexOf("=", StringComparison.Ordinal);
                int end = temp.IndexOf("|", StringComparison.Ordinal);
                string result = temp.Substring(start + 1, end - start - 1);
                return result.Trim();
            }
            catch (Exception exception)
            {
                Logger.Error(exception+"|Error At:"+message, "Parser.StringValue", "StringValue");
                return null;
            }
        }
        public static decimal DecimalValue(string message)
        {
            string temp = message.TrimStart('|');
            int start = temp.IndexOf("=", StringComparison.Ordinal);
            int end = temp.IndexOf("|", StringComparison.Ordinal);
            string result = temp.Substring(start + 1, end - start - 1);
            return Convert.ToDecimal(result.Trim());
        }
        public static string RegularExp(string tag)
        {
            return "(\\|" + tag + "=\\w+\\.?(\\w+)?\\|)";
        }
    }
}
