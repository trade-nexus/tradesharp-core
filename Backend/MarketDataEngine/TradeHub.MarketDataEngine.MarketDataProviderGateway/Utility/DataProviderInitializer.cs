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
using System.Xml;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.MarketDataProvider;

namespace TradeHub.MarketDataEngine.MarketDataProviderGateway.Utility
{
    /// <summary>
    /// Initializes the Required Market Data Provider instance
    /// </summary>
    public static class DataProviderInitializer
    {
        private static Type _type = typeof (DataProviderInitializer);

        /// <summary>
        /// Provides the Market Data Provider instance depending on the specified provider
        /// </summary>
        public static IMarketDataProvider GetMarketDataProviderInstance(string providerName)
        {
            try
            {
                var doc = new XmlDocument();

                // Read RabbitMQ configuration file
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\AvailableProviders.xml");

                // Read the specified Node value
                XmlNode providerInfo = doc.SelectSingleNode(xpath: "Providers/" + providerName);

                // Check if the Requested Provider info is available
                if (providerInfo == null)
                {
                    if(Logger.IsInfoEnabled)
                    {
                        Logger.Info("Requested Market Data Provider not available.", _type.FullName, "GetMarketDataProviderInstance");
                    }
                    return null;
                }

                IMarketDataProvider marketDataProvider = ContextRegistry.GetContext()[providerName + "MarketDataProvider"] as IMarketDataProvider;

                return marketDataProvider;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "GetMarketDataProviderInstance");
                return null;
            }
        }
    }
}
