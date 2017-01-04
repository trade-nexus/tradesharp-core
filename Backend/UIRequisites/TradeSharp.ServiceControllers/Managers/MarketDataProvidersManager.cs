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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TraceSourceLogger;
using TradeSharp.UI.Common.Constants;
using TradeSharp.UI.Common.Infrastructure;
using TradeSharp.UI.Common.Models;

namespace TradeSharp.ServiceControllers.Managers
{
    /// <summary>
    /// Handles Market Data Provider's related Admin functionality
    /// </summary>
    internal class MarketDataProvidersManager
    {
        private Type _type = typeof(MarketDataProvidersManager);

        /// <summary>
        /// Directory path at which Market Data Provider's files are located
        /// </summary>
        private readonly string _marketDataProvidersRootFolderPath;

        /// <summary>
        /// Directory path at which Market Data Provider's Config files are located
        /// </summary>
        private readonly string _marketDataProvidersConfigFolderPath;

        /// <summary>
        /// File name which holds the name of all available market data providers
        /// </summary>
        private readonly string _marketDataProvidersFileName;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MarketDataProvidersManager()
        {
            _marketDataProvidersRootFolderPath = DirectoryPath.MARKETDATA_ENGINE_PATH;
            _marketDataProvidersConfigFolderPath = _marketDataProvidersRootFolderPath + @"Config\";

            _marketDataProvidersFileName = "AvailableProviders.xml";
        }

        /// <summary>
        /// Returns a list of available market data providers
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, List<ProviderCredential>> GetAvailableProviders()
        {
            // File Saftey Check
            if (!File.Exists(_marketDataProvidersConfigFolderPath + _marketDataProvidersFileName)) 
                return null;

            // Will hold credential information against each availale provider
            IDictionary<string, List<ProviderCredential>> availableProviders = new Dictionary<string, List<ProviderCredential>>();

            // XML Document to read file
            var availableProvidersDocument = new XmlDocument();

            // Read file to get available provider's names.
            availableProvidersDocument.Load(_marketDataProvidersConfigFolderPath + _marketDataProvidersFileName);

            // Read the all Node value
            XmlNodeList providersInfo = availableProvidersDocument.SelectNodes(xpath: "Providers/*");

            if (providersInfo != null)
            {
                // Extract individual attribute value
                foreach (XmlNode node in providersInfo)
                {
                    // Create file name from which to read Provider Credentials
                    string credentialsFileName = node.Name + @"Params.xml";

                    // XML Document to read provider specific xml file
                    var availableCredentialsDoc = new XmlDocument();

                    // Holds extracted credentials from the xml file
                    var providerCredentialList = new List<ProviderCredential>();

                    if (File.Exists(_marketDataProvidersConfigFolderPath + credentialsFileName))
                    {
                        // Read configuration file
                        availableCredentialsDoc.Load(_marketDataProvidersConfigFolderPath + credentialsFileName);

                        // Read all the parametes defined in the configuration file
                        XmlNodeList configNodes = availableCredentialsDoc.SelectNodes(xpath: node.Name + "/*");
                        if (configNodes != null)
                        {
                            // Extract individual attribute value
                            foreach (XmlNode innerNode in configNodes)
                            {
                                ProviderCredential providerCredential = new ProviderCredential();

                                providerCredential.CredentialName = innerNode.Name;
                                providerCredential.CredentialValue = innerNode.InnerText;

                                // Add to Credentials list
                                providerCredentialList.Add(providerCredential);
                            }
                        }
                    }
                    // Add all details to providers info map
                    availableProviders.Add(node.Name, providerCredentialList);
                }
            }

            return availableProviders;
        }

        /// <summary>
        /// Edits given Market data provider credentails with the new values
        /// </summary>
        /// <param name="provider">Contains provider details</param>
        public bool EditProviderCredentials(Provider provider)
        {
            try
            {
                bool valueSaved = false;

                // Create file path
                string filePath = _marketDataProvidersConfigFolderPath + provider.ProviderName + @"Params.xml";

                // Create XML Document Object to read credentials file
                XmlDocument document = new XmlDocument();

                // Load credentials file
                document.Load(filePath);

                XmlNode root = document.DocumentElement;

                // Travers all credential values
                foreach (ProviderCredential providerCredential in provider.ProviderCredentials)
                {
                    XmlNode xmlNode = root.SelectSingleNode("descendant::" + providerCredential.CredentialName);

                    if (xmlNode != null)
                    {
                        xmlNode.InnerText = providerCredential.CredentialValue;

                        valueSaved = true;
                    }
                }

                // Save document
                document.Save(filePath);

                return valueSaved;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "EditProviderCredentials");
                return false;
            }
        }

        /// <summary>
        /// Adds given provider to Market Data Engine - Server
        /// </summary>
        /// <param name="connectorPath">Complete path for connectors library</param>
        /// <param name="providerName">Name to be used for the given connector</param>
        public Tuple<bool, string> AddProvider(string connectorPath, string providerName)
        {
            // Get root directory path
            string rootDirectory = Path.GetDirectoryName(connectorPath);
            // Get Config folder path
            string configPath = rootDirectory + @"\Config";
            // Get Spring file path
            string springFileName = providerName + "SpringConfig.xml";
            string springFilePath = configPath + @"\" + springFileName;

            if (!VerifySpringConfigFileName(springFilePath))
                return new Tuple<bool, string>(false, "Expected Spring Configuration file not found.");

            if (!CopyProviderLibraries(connectorPath))
                return new Tuple<bool, string>(false, "Given files were not copied to the Server location.");

            if (!ModifyServerSpringParameters(springFileName))
                return new Tuple<bool, string>(false, "Spring configuration was not modified.");

            if (!AddProviderName(providerName))
                return new Tuple<bool, string>(false, "Not able to add new Provider name to Server.");

            return new Tuple<bool, string>(true, "Provider is sucessfully added to the Server.");
        }

        /// <summary>
        /// Removes given provider from the Market Data Engine - Server
        /// </summary>
        /// <param name="provider">Contains complete provider details</param>
        /// <returns></returns>
        public Tuple<bool, string> RemoveProvider(Provider provider)
        {
            if (!RemoveProviderName(provider.ProviderName))
                return new Tuple<bool, string>(false, "Not able to remove Provider name from Server.");

            return new Tuple<bool, string>(true, "Provider is sucessfully removed from the Server.");
        }

        /// <summary>
        /// Verifies if the valid spring config file name is provided for the given connector
        /// </summary>
        /// <param name="springFile">Complete path for Spring configuration file</param>
        /// <returns></returns>
        private bool VerifySpringConfigFileName(string springFile)
        {
            try
            {
                return File.Exists(springFile);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "VerifySpringConfigFileName");
                return false;
            }
        }

        /// <summary>
        /// Copies given provider and its dependencies to Market Data Engine - Server
        /// </summary>
        /// <param name="path">Directory path for Provider</param>
        /// <returns></returns>
        private bool CopyProviderLibraries(string path)
        {
            try
            {
                // Get all files information in the given directory
                string[] files = Directory.GetFiles(Path.GetDirectoryName(path), "*", SearchOption.AllDirectories);

                // Copy individual Files
                foreach (string file in files)
                {
                    if (File.Exists(file)) continue;

                    File.Copy(file, _marketDataProvidersRootFolderPath + Path.GetFileName(file), false);
                }

                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "CopyProviderLibraries");
                return false;
            }
        }

        /// <summary>
        /// Modifies Server Spring parameters by adding given providers spring object in the spring config
        /// </summary>
        /// <param name="springFileName"></param>
        /// <returns></returns>
        private bool ModifyServerSpringParameters(string springFileName)
        {
            try
            {
                string appConfigName = "TradeHub.MarketDataEngine.Server.Console.exe.config";
                string appConfigPath = _marketDataProvidersRootFolderPath + appConfigName;

                // Modify configuration file
                return XmlFileManager.ModifyAppConfigForSpringObject(appConfigPath, @"~/Config/" + springFileName);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ModifyServerSpringParameters");
                return false;
            }
        }

        /// <summary>
        /// Adds given provider name to Market Data Engine - Server
        /// </summary>
        /// <param name="providerName"></param>
        /// <returns></returns>
        private bool AddProviderName(string providerName)
        {
            try
            {
                string path = _marketDataProvidersConfigFolderPath + _marketDataProvidersFileName;

                return XmlFileManager.AddChildNode(path, providerName);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "AddProviderName");
                return false;
            }
        }

        /// <summary>
        /// Removes given provider name from Market Data Engine - Server
        /// </summary>
        /// <param name="providerName"></param>
        /// <returns></returns>
        private bool RemoveProviderName(string providerName)
        {
            try
            {
                string path = _marketDataProvidersConfigFolderPath + _marketDataProvidersFileName;

                return XmlFileManager.RemoveChildNode(path, providerName);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "AddProviderName");
                return false;
            }
        }
    }
}
