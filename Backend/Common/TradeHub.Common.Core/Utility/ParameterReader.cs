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
using System.Xml;
using TraceSourceLogger;

namespace TradeHub.Common.Core.Utility
{
    /// <summary>
    /// Read provider paramater from file
    /// </summary>
    public static class ParameterReader
    {
        private static Type _type = typeof (ParameterReader);

        /// <summary>
        /// Reads parameters from the configuration file
        /// </summary>
        /// <param name="paramsFileName"></param>
        /// <param name="parentNode"></param>
        public static Dictionary<string, string> ReadParamters(string paramsFileName, string parentNode)
        {
            Dictionary<string, string> parameters= new Dictionary<string, string>();

            try
            {
                var doc = new XmlDocument();

                // Read configuration file
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + paramsFileName);

                // Read all the parametes defined in the configuration file
                XmlNodeList configNodes = doc.SelectNodes(xpath: parentNode+"/*");
                if (configNodes != null)
                {
                    // Extract individual attribute value
                    foreach (XmlNode node in configNodes)
                    {
                        parameters.Add(node.Name, node.InnerText);
                    }
                }

                // Log parameters
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(parameters.ToString(), _type.FullName, "ReadParamters");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadParameters");
            }

            return parameters;
        }
    }
}
