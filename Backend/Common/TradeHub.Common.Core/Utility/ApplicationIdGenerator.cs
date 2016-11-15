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
using TraceSourceLogger;

namespace TradeHub.Common.Core.Utility
{
    /// <summary>
    /// Generates Unique IDs to be assigned to connecting Applications
    /// </summary>
    public static class ApplicationIdGenerator
    {
        private static Type _type = typeof (ApplicationIdGenerator);

        private const int Min = 0xA00;
        private const int Max = 0xFF9;
        private static int _value = Min - 1;
        
        // Contains the IDs which are currently in use
        private static List<string> _idsInUse = new List<string>();


        /// <summary>
        /// Provides New Valid ID
        /// </summary>
        /// <returns></returns>
        public static string NextId()
        {
            if (_value < Max)
            {
                _value++;
            }
            else
            {
                _value = Min;
            }
            return _value.ToString("X");
        }

        /// <summary>
        /// Adds the New Generated ID to the local IDs Map
        /// </summary>
        /// <param name="id">Unqiue ID</param>
        private static bool AddNewId(string id)
        {
            try
            {
                if (!_idsInUse.Contains(id))
                {
                    _idsInUse.Add(id);
                    return true;
                }
                return false;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "AddNewId");
                return false;
            }
        }

        /// <summary>
        /// Removes specified ID from the local IDs Map
        /// </summary>
        /// <param name="id">Unqiue ID</param>
        private static bool RemoveId(string id)
        {
            try
            {
                if (_idsInUse.Remove(id))
                {
                    return true;
                }
                return false;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RemoveId");
                return false;
            }
        }
    }
}
