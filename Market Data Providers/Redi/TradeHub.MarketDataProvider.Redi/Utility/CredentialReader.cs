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
using System.Xml;
using TraceSourceLogger;
using TradeHub.MarketDataProvider.Redi.ValueObject;

namespace TradeHub.MarketDataProvider.Redi.Utility
{
    /// <summary>
    /// Provides Parameters required to establish the Tradier Connection
    /// </summary>
    public static class CredentialReader
    {
        private static readonly Type _type = typeof(CredentialReader);

        /// <summary>
        /// Reads parameters from the configuration file
        /// </summary>
        public static Credentials ReadCredentials(String paramsFileName)
        {
            try
            {
                // Create new object to hold credential details
                Credentials credentials = new Credentials();

                // Create XML document to read condfiguration
                var doc = new XmlDocument();

                // Read configuration file
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + paramsFileName);

                // Read all the parametes defined in the configuration file
                XmlNodeList configNodes = doc.SelectNodes(xpath: "Redi/*");
                if (configNodes != null)
                {
                    // Extract individual attribute value
                    foreach (XmlNode node in configNodes)
                    {
                        AddParameters(credentials, node.Name, node.InnerText);
                    }
                }

                // Log parameters
                if(Logger.IsInfoEnabled)
                {
                    Logger.Info(credentials.ToString(), _type.FullName, "ReadCredentials");
                }

                return credentials;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadCredentials");
                return null;
            }
        }

        /// <summary>
        /// Adds the value to the matching Attributes property
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        private static void AddParameters(Credentials credentials, string parameterName, string parameterValue)
        {
            if(Logger.IsDebugEnabled)
            {
                Logger.Debug("Adding attribute :: " + parameterName + ":" + parameterValue, _type.FullName, "AddAttributes");
            }
            
            switch (parameterName.Trim().ToLowerInvariant())
            {
                case "username":
                    credentials.Username = parameterValue.Trim();
                    break;
                case "password":
                    credentials.Password = parameterValue.Trim();
                    break;
                case "ipaddress":
                    credentials.IpAddress = parameterValue.Trim();
                    break;
                case "port":
                    credentials.Port = parameterValue.Trim();
                    break;
            }
        }
    }
}
