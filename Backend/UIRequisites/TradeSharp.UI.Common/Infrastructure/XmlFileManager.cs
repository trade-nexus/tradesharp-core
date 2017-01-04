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


using System;
using System.Xml;
using TraceSourceLogger;

namespace TradeSharp.UI.Common.Infrastructure
{
    /// <summary>
    /// Provides functionality to Read/Modify given XML files
    /// </summary>
    public static class XmlFileManager
    {
        private static Type _type = typeof(XmlFileManager);

        /// <summary>
        /// Adds a new child node to the given XML file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="childNodeName"></param>
        /// <returns></returns>
        public static bool AddChildNode(string filePath, string childNodeName)
        {
            try
            {
                bool valueSaved = false;

                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);

                XmlNode root = doc.DocumentElement;

                if (root != null)
                {
                    //Create a new node.
                    XmlElement resourceNodeElement = doc.CreateElement(childNodeName);

                    // Add child node
                    root.AppendChild(resourceNodeElement);

                    valueSaved = true;
                    doc.Save(filePath);
                }

                return valueSaved;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "AddChildNode");
                return false;
            }
        }

        /// <summary>
        /// Adds a new child node to the given XML file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="childNodeName"></param>
        /// <returns></returns>
        public static bool RemoveChildNode(string filePath, string childNodeName)
        {
            try
            {
                bool valueSaved = false;

                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);

                XmlNode root = doc.DocumentElement;

                if (root != null)
                {
                    // Get Context Node
                    XmlNode childNode = root.SelectSingleNode("descendant::" + childNodeName);

                    // Add child node
                    if (childNode != null)
                    {
                        root.RemoveChild(childNode);

                        valueSaved = true;
                        doc.Save(filePath);
                    }
                }

                return valueSaved;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "AddChildNode");
                return false;
            }
        }

        /// <summary>
        /// Returns required values from the given file
        /// </summary>
        /// <param name="path">.csv File Path</param>
        /// <returns></returns>
        public static Tuple<string, string, string> GetHistoricalParameters(string path)
        {
            try
            {
                string startDate = "";
                string endDate = "";
                string providerName = "";

                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlNode root = doc.DocumentElement;

                if (root != null)
                {
                    // Read Start Date value
                    XmlNode startNode = root.SelectSingleNode("descendant::StartDate");

                    if (startNode != null)
                    {
                        startDate = startNode.InnerText;
                    }

                    // Read End Date value
                    XmlNode endNode = root.SelectSingleNode("descendant::EndDate");

                    if (endNode != null)
                    {
                        endDate = endNode.InnerText;
                    }

                    // Read Provider Name
                    XmlNode providerNode = root.SelectSingleNode("descendant::Provider");

                    if (providerNode != null)
                    {
                        providerName = providerNode.InnerText;
                    }

                    // Return values
                    return new Tuple<string, string, string>(startDate, endDate, providerName);
                }

                return null;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "GetHistoricalParameters");
                return null;
            }
        }

        /// <summary>
        /// Saves values in the given file
        /// </summary>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="providerName">Name of the Provider</param>
        /// <param name="path">.csv File Path</param>
        public static void SaveHistoricalParameters(string startDate, string endDate, string providerName, string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlNode root = doc.DocumentElement;

                if (root != null)
                {
                    // Save Start Date
                    XmlNode startNode = root.SelectSingleNode("descendant::StartDate");
                    if (startNode != null)
                    {
                        startNode.InnerText = startDate;
                    }

                    // Save End Date
                    XmlNode endNode = root.SelectSingleNode("descendant::EndDate");
                    if (endNode != null)
                    {
                        endNode.InnerText = endDate;
                    }

                    // Save Provider Name
                    XmlNode providerNode = root.SelectSingleNode("descendant::Provider");
                    if (providerNode != null)
                    {
                        providerNode.InnerText = providerName;
                    }

                    doc.Save(path);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SaveHistoricalParameters");
            }
        }

        /// <summary>
        /// Modifies given App.Config file to add a new Spring object 
        /// </summary>
        public static bool ModifyAppConfigForSpringObject(string appConfigPath, string springObject)
        {
            try
            {
                bool valueSaved = false;

                XmlDocument doc = new XmlDocument();
                doc.Load(appConfigPath);

                XmlNode root = doc.DocumentElement;

                if (root != null)
                {
                    // Get Context Node
                    XmlNode contextNode = root.SelectSingleNode("descendant::context");

                    if (contextNode != null)
                    {
                        // Create a new node content
                        XmlElement resourceNodeElement = doc.CreateElement("resource");

                        // Add newly created node
                        contextNode.InsertBefore(resourceNodeElement, contextNode.FirstChild);

                        // Set node attributes
                        XmlAttribute newAttribute = doc.CreateAttribute("uri");
                        newAttribute.Value = springObject;
                        resourceNodeElement.Attributes.Append(newAttribute);

                        valueSaved = true;
                    }

                    doc.Save(appConfigPath);
                }

                return valueSaved;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ModifyAppConfigForSpringObject");
                return false;
            }
        }
    }
}
