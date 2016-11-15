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

namespace TradeHub.TradeManager.Client.Utility
{
    /// <summary>
    /// Reads Message Queue's configuration parameters to be used for Trade Manager Client
    /// </summary>
    public class MqConfigurationReader
    {
        private static Type _type = typeof (MqConfigurationReader);

        /// <summary>
        /// Name of SERVER - Trade Manager MQ properties file
        /// </summary>
        private readonly string _serverConfig;

        /// <summary>
        /// Name of CLIENT - Trade Manager MQ properties file
        /// </summary>
        private readonly string _clientConfig;

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        private Dictionary<string, string> _serverMqParameters;

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        private Dictionary<string, string> _clientMqParameters;

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        public IReadOnlyDictionary<string, string> ServerMqParameters
        {
            get { return _serverMqParameters; }
        }

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        public IReadOnlyDictionary<string, string> ClientMqParameters
        {
            get { return _clientMqParameters; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="serverConfig">Name of SERVER - Trade Manager MQ properties file</param>
        /// <param name="clientConfig">Name of CLIENT - Trade Manager MQ properties file</param>
        public MqConfigurationReader(string serverConfig, string clientConfig)
        {
            // Save information
            _serverConfig = serverConfig;
            _clientConfig = clientConfig;

            // Initialize values
            _clientMqParameters = new Dictionary<string, string>();
            _serverMqParameters = new Dictionary<string, string>();

            // Read Parameters
            ReadTradeManagerServerMqProperties();
            ReadTradeManagerClientMqProperties();
        }

        /// <summary>
        /// Reads Trade Manager MQ parameters from the Config file
        /// </summary>
        private void ReadTradeManagerServerMqProperties()
        {
            try
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + _serverConfig))
                {
                    var doc = new XmlDocument();

                    // Read Specified configuration file
                    doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + _serverConfig);

                    // Read the specified Node values
                    XmlNodeList nodes = doc.SelectNodes(xpath: "RabbitMQ/*");
                    if (nodes != null)
                    {
                        foreach (XmlNode node in nodes)
                        {
                            // Add value to the dictionary
                            _serverMqParameters.Add(node.Name, node.InnerText);
                        }
                    }
                    return;
                }
                Logger.Info("File not found: " + _serverConfig, _type.FullName, "ReadTradeManagerServerMqProperties");
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadTradeManagerServerMqProperties");
            }
        }

        /// <summary>
        /// Reads Trade Manager MQ parameters from the Config file
        /// </summary>
        private void ReadTradeManagerClientMqProperties()
        {
            try
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + _clientConfig))
                {
                    var doc = new XmlDocument();

                    // Read Specified configuration file
                    doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + _clientConfig);

                    // Read the specified Node values
                    XmlNodeList nodes = doc.SelectNodes(xpath: "ClientRabbitMQ/*");
                    if (nodes != null)
                    {
                        foreach (XmlNode node in nodes)
                        {
                            // Add value to the dictionary
                            _clientMqParameters.Add(node.Name, node.InnerText);
                        }
                    }
                    return;
                }
                Logger.Info("File not found: " + _clientConfig, _type.FullName, "ReadTradeManagerClientMqProperties");
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadTradeManagerClientMqProperties");
            }
        }
    }
}
