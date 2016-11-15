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

namespace TradeHub.StrategyRunner.UserInterface.SettingsModule.Utility
{
    /// <summary>
    /// Provides functionality to Read/Modify given XML files
    /// </summary>
    public static class XmlFileHandler
    {
        private static Type _type = typeof (XmlFileHandler);

        /// <summary>
        /// Returns required values from the given file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Tuple<string,string> GetValues(string path)
        {
            try
            {
                string startDate = "";
                string endDate = "";

                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlNode root = doc.DocumentElement;

                XmlNode startNode = root.SelectSingleNode("descendant::StartDate");

                if (startNode != null)
                {
                    startDate = startNode.InnerText;
                }

                XmlNode endNode = root.SelectSingleNode("descendant::EndDate");

                if (endNode != null)
                {
                    endDate = endNode.InnerText;
                }

                return new Tuple<string, string>(startDate, endDate);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "GetValues");
                return new Tuple<string, string>("", "");
            }
        }

        /// <summary>
        /// Saves values in the given file
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="path"></param>
        public static void SaveValues(string startDate, string endDate, string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlNode root = doc.DocumentElement;

                XmlNode startNode = root.SelectSingleNode("descendant::StartDate");

                if (startNode != null)
                {
                    startNode.InnerText = startDate;
                }

                XmlNode endNode = root.SelectSingleNode("descendant::EndDate");

                if (endNode != null)
                {
                    endNode.InnerText = endDate;
                }

                doc.Save(path);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SaveValues");
            }
        }
    }
}
