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
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Fix.Constants;

namespace TradeHub.Common.Fix.Converter
{
    public static class ConvertTif
    {
        /// <summary>
        /// Take TradeHub TIF value and returns FIX variant
        /// </summary>
        /// <param name="tif"></param>
        /// <returns></returns>
        public static char GetFixValue(string tif)
        {
            switch (tif)
            {
                case OrderTif.DAY:
                    return TimeInForce.DAY;
                case OrderTif.GTC:
                    return TimeInForce.GTC;
                case OrderTif.GTD:
                    return TimeInForce.GTD;
                case OrderTif.GTX:
                    return TimeInForce.GTX;
                case OrderTif.IOC:
                    return TimeInForce.IOC;
                case OrderTif.OPG:
                    return TimeInForce.OPG;
                case OrderTif.FOK:
                    return TimeInForce.FOK;
                default:
                    return ' ';
            }
        }

        /// <summary>
        /// Take FIX TIF value and returns TradeHUB variant
        /// </summary>
        /// <param name="tif"></param>
        /// <returns></returns>
        public static string GetLocalValue(char tif)
        {
            switch (tif)
            {
                case TimeInForce.DAY:
                    return OrderTif.DAY;
                case TimeInForce.GTC:
                    return OrderTif.GTC;
                case TimeInForce.GTD:
                    return OrderTif.GTD;
                case TimeInForce.GTX:
                    return OrderTif.GTX;
                case TimeInForce.IOC:
                    return OrderTif.IOC;
                case TimeInForce.OPG:
                    return OrderTif.OPG;
                case TimeInForce.FOK:
                    return OrderTif.FOK;
                default:
                    return OrderTif.NONE;
            }
        }
    }
}
