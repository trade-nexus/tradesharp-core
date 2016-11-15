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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TraceSourceLogger;

namespace TradeHub.NotificationEngine.Common.Utility
{
    /// <summary>
    /// Reads Message Queue's configuration parameters to be used by Notification Engine
    /// </summary>
    public static class MqConfigurationReader
    {
        private static Type _type = typeof (MqConfigurationReader);

        /// <summary>
        /// Reads Server MQ parameters from the Config file
        /// </summary>
        /// <param name="serverConfig">File name from which to read settings</param>
        public static Dictionary<string, string> ReadServerMqProperties(string serverConfig)
        {
            try
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + serverConfig))
                {
                    var doc = new XmlDocument();

                    // Read Specified configuration file
                    doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + serverConfig);

                    // Read the specified Node values
                    XmlNodeList nodes = doc.SelectNodes(xpath: "RabbitMQ/*");

                    // Create dictionary to hold parameter information
                    Dictionary<string, string> serverMqParameters= new Dictionary<string, string>();

                    if (nodes != null)
                    {
                        foreach (XmlNode node in nodes)
                        {
                            // Add value to the dictionary
                            serverMqParameters.Add(node.Name, node.InnerText);
                        }
                    }
                    return serverMqParameters;
                }
                Logger.Info("File not found: " + serverConfig, _type.FullName, "ReadServerMqProperties");
                return null;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadServerMqProperties");
                return null;
            }
        }

        /// <summary>
        /// Reads Client MQ parameters from the Config file
        /// <param name="clientConfig">File name from which to read settings</param>
        /// </summary>
        public static Dictionary<string, string> ReadClientMqProperties(string clientConfig)
        {
            try
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + clientConfig))
                {
                    var doc = new XmlDocument();

                    // Read Specified configuration file
                    doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + clientConfig);

                    // Read the specified Node values
                    XmlNodeList nodes = doc.SelectNodes(xpath: "ClientRabbitMQ/*");

                    // Create dictionary to hold parameter information
                    Dictionary<string, string> clientMqParameters = new Dictionary<string, string>();
                    if (nodes != null)
                    {
                        foreach (XmlNode node in nodes)
                        {
                            // Add value to the dictionary
                            clientMqParameters.Add(node.Name, node.InnerText);
                        }
                    }
                    return clientMqParameters;
                }
                
                Logger.Info("File not found: " + clientConfig, _type.FullName, "ReadClientMqProperties");
                return null;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadClientMqProperties");
                return null;
            }
        }
    }
}
