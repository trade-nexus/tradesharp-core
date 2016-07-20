using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TraceSourceLogger;

namespace TradeHub.PositionEngine.Client.Utility
{
    /// <summary>
    /// Reads required parameters from the specified Config file
    /// </summary>
    public class ConfigurationReader
    {
        private Type _type = typeof(ConfigurationReader);
     
        // Name of OEE Server Configuration File
        private readonly string _oeeServerConfig;
        // Name of Strategy Gateway Configuration File
        private readonly string _clientConfig;

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        private Dictionary<string, string> _peMqServerparameters;

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        private Dictionary<string, string> _clientMqParameters;

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        public Dictionary<string, string> PeMqServerparameters
        {
            get { return _peMqServerparameters; }
            set { _peMqServerparameters = value; }
        }

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        public Dictionary<string, string> ClientMqParameters
        {
            get { return _clientMqParameters; }
            set { _clientMqParameters = value; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public ConfigurationReader(string oeeServerConfig, string clientConfig)
        {
            _oeeServerConfig = oeeServerConfig;
            _clientConfig = clientConfig;

            // Initialize values
            _clientMqParameters = new Dictionary<string, string>();
            _peMqServerparameters = new Dictionary<string, string>();

            // Read Parameters
            ReadPeMqServerConfigSettings();
            ReadClientMqConfigSettings();
        }

        /// <summary>
        /// Reads OEE MQ parameters from the Config file
        /// </summary>
        private void ReadPeMqServerConfigSettings()
        {
            try
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + _oeeServerConfig))
                {
                    var doc = new XmlDocument();

                    // Read Specified configuration file
                    doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + _oeeServerConfig);

                    // Read the specified Node values
                    XmlNodeList nodes = doc.SelectNodes(xpath: "RabbitMQ/*");
                    if (nodes != null)
                    {
                        foreach (XmlNode node in nodes)
                        {
                            // Add value to the dictionary
                            _peMqServerparameters.Add(node.Name, node.InnerText);
                            Logger.Info("Adding parameter: " + node.Name + " | Value: " + node.InnerText, _type.FullName, "ReadMdeMqConfigSettings");
                        }
                    }
                    return;
                }
                Logger.Info("File not found: " + _oeeServerConfig, _type.FullName, "ReadMdeMqConfigSettings");
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadMdeMqConfigSettings");
            }
        }

        /// <summary>
        /// Reads Strategy Gateway MQ parameters from the Config file
        /// </summary>
        private void ReadClientMqConfigSettings()
        {
            try
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + _clientConfig))
                {
                    // Create GUID to be used for Inquiry Queue
                    string inquiryQueueId = Guid.NewGuid().ToString();

                    var doc = new XmlDocument();

                    // Read Specified configuration file
                    doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + _clientConfig);

                    // Read the specified Node values
                    XmlNodeList nodes = doc.SelectNodes(xpath: "PositionMQParameters/*");
                    if (nodes != null)
                    {
                        foreach (XmlNode node in nodes)
                        {
                            if (node.Name.Equals(Constants.PeClientMqParameters.InquiryResponseQueue))
                            {
                                node.InnerText = inquiryQueueId + "_queue";
                            }
                            else if (node.Name.Equals(Constants.PeClientMqParameters.InquiryResponseRoutingKey))
                            {
                                node.InnerText = inquiryQueueId + ".routingkey";
                            }
                            // Add value to the dictionary
                            _clientMqParameters.Add(node.Name, node.InnerText);
                            Logger.Info("Adding parameter: " + node.Name + " | Value: " + node.InnerText, _type.FullName, "ReadClientMqConfigSettings");
                        }
                    }
                    return;
                }
                Logger.Info("File not found: " + _clientConfig, _type.FullName, "ReadClientMqConfigSettings");
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadStrategyMqConfigSettings");
            }
        }
    }
}
