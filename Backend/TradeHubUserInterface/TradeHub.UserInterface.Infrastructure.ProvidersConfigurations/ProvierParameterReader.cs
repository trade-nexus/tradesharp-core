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
using TradeHub.UserInterface.Common;

namespace TradeHub.UserInterface.Infrastructure.ProvidersConfigurations
{
    static class ProvierParameterReader
    {
        private static Type _type = typeof (ProvierParameterReader);
 
        public static void LoadParamerters(ServiceProvider provider)
        {
            string filePath = GetFilePath(provider);
            try
            {
                if (!filePath.Equals(string.Empty))
                {
                    List<Parameters> parameterses = ReadParamters(filePath);
                    EventSystem.Publish<List<Parameters>>(parameterses);

                }

            }
            catch (Exception exception)
            {

                Logger.Error(exception, _type.FullName, "LoadParamerters");
            }
        }

        /// <summary>
        /// Get the required file path.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        private static string  GetFilePath(ServiceProvider provider)
        {
            string filePath = string.Empty;

            if (provider.ServiceName.Equals("Market Data Engine"))
            {
                filePath =@"..\Market Data Engine\Config\"+provider.ProviderName+"Params.xml";
            }
            else if (provider.ServiceName.Equals("Order Execution Engine"))
            {
                filePath = @"..\Order Execution Engine\Config\"+provider.ProviderName+"OrderParams.xml";
            }

            //if (provider.ProviderName == "Blackwood")
            //{
            //    filePath = filePath + "BlackwoodOrderParams.xml";
            //}


            return filePath;

        }

        /// <summary>
        /// Read all the parameters from file.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private static List<Parameters> ReadParamters(string filepath)
        {
            List<Parameters> parameterses=null;
            try
            {
                var doc = new XmlDocument();

                // Read configuration file
                doc.Load(filepath);

                
                // Read all the parametes defined in the configuration file
                XmlNodeList configNodes = doc.SelectNodes(xpath: "Blackwood/*");
                if (configNodes != null)
                {
                    parameterses=new List<Parameters>();
                    // Extract individual attribute value
                    foreach (XmlNode node in configNodes)
                    {
                        Parameters parameters = new Parameters(node.Name, node.InnerText);
                        parameterses.Add(parameters);
                    }
                }

              
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadParameters");
            }
            return parameterses;
        }

        /// <summary>
        /// Save the parameters.
        /// </summary>
        public static void SaveParameters(ServiceParametersList parametersList)
        {
            string filepath = GetFilePath(parametersList.ServiceProvider);

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode rootNode = xmlDoc.CreateElement("Blackwood");
                xmlDoc.AppendChild(rootNode);

                foreach (var parameter in parametersList.ParametersList)
                {
                    XmlNode userNode = xmlDoc.CreateElement(parameter.ParameterName);
                    if (parameter.ParameterName.Equals("username",StringComparison.CurrentCultureIgnoreCase))
                        parameter.ParameterValue = parameter.ParameterValue.ToUpper();
                    userNode.InnerText = parameter.ParameterValue;
                    rootNode.AppendChild(userNode);
                }
                
                xmlDoc.Save(filepath);

            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadParameters");
            }
        }
    }
}
