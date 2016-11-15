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

namespace TradeHub.StrategyEngine.Common.Utility
{
    public static class ObjectInitializer
    {
        public static object CastObject(string input, string type)
        {
            switch (type)
            {
                case "UInt32": // uint
                    UInt32 outputUInt32;
                    if (UInt32.TryParse(input, out outputUInt32))
                        return outputUInt32;
                    else
                        return null;

                case "Int32": // int
                    Int32 output32;
                    if(Int32.TryParse(input, out output32))
                        return output32;
                    else
                        return null;

                case "UInt16": // ushort
                    UInt16 outputUInt16;
                    if (UInt16.TryParse(input, out outputUInt16))
                        return outputUInt16;
                    else
                        return null;

                case "Int16": // short
                    Int16 outputInt16;
                    if (Int16.TryParse(input, out outputInt16))
                        return outputInt16;
                    else
                        return null;

                case "UInt64": //ulong
                    UInt64 outputUInt64;
                    if (UInt64.TryParse(input, out outputUInt64))
                        return outputUInt64;
                    else
                        return null;

                case "Int64": // long
                    Int64 output64;
                    if (Int64.TryParse(input, out output64))
                        return output64;
                    else
                        return null;

                case "Decimal": // decimal
                    decimal output;
                    if (decimal.TryParse(input, out output))
                        return output;
                    else
                        return null;

                case "Double": // double
                    double outputDouble;
                    if (double.TryParse(input, out outputDouble))
                        return outputDouble;
                    else
                        return null;

                case "Single": // Float
                    Single outputSingle;
                    if (float.TryParse(input, out outputSingle))
                        return outputSingle;
                    else
                        return null;

                case "Char": // char
                    return Convert.ToChar(input);

                case "Boolean": // bool
                    return Convert.ToBoolean(input);

                case "String": // string
                    return input;
                default:
                    return null;
            }
        }
    }
}
