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
using TraceSourceLogger;

namespace TradeHub.NotificationEngine.Common.Utility
{
    public class NotificationConfigurationWriter
    {
        private static Type _type = typeof (NotificationConfigurationWriter);

        /// <summary>
        /// Writes email parameters to the configuration file
        /// </summary>
        /// <param name="path">configuration file path</param>
        /// <param name="parameters">email parameters i,e. Item1=username, Item2=Password</param>
        public static void WriteEmailConfiguration(string path, Tuple<string, string> parameters)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlNode root = doc.DocumentElement;

                if (root != null)
                {
                    // Save Username
                    XmlNode usernameNode = root.SelectSingleNode("descendant::username");
                    if (usernameNode != null)
                    {
                        usernameNode.InnerText = parameters.Item1;
                    }

                    // Save Password
                    XmlNode passwordNode = root.SelectSingleNode("descendant::password");
                    if (passwordNode != null)
                    {
                        passwordNode.InnerText = parameters.Item2;
                    }

                    doc.Save(path);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "WriteEmailConfiguration");
            }
        }
    }
}
