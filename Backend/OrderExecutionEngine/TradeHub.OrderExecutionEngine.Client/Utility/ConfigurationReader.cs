using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Spring.Context.Support;
using TraceSourceLogger;

namespace TradeHub.OrderExecutionEngine.Client.Utility
{
    /// <summary>
    /// Reads required parameters from the specified Config file
    /// </summary>
    public class ConfigurationReader
    {
        private Type _type = typeof(ConfigurationReader);
        private AsyncClassLogger _asyncClassLogger;

        // Name of OEE Server Configuration File
        private readonly string _oeeServerConfig;
        // Name of Strategy Gateway Configuration File
        private readonly string _clientConfig;

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        private Dictionary<string, string> _oeeMqServerparameters;

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        private Dictionary<string, string> _clientMqParameters;

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        public Dictionary<string, string> OeeMqServerparameters
        {
            get { return _oeeMqServerparameters; }
            set { _oeeMqServerparameters = value; }
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
        public ConfigurationReader(string oeeServerConfig, string clientConfig, AsyncClassLogger asyncClassLogger)
        {
            _asyncClassLogger = asyncClassLogger;
            _oeeServerConfig = oeeServerConfig;
            _clientConfig = clientConfig;

            // Initialize values
            _clientMqParameters = new Dictionary<string, string>();
            _oeeMqServerparameters = new Dictionary<string, string>();
        }

        /// <summary>
        /// Reads configuration parameters
        /// </summary>
        public void ReadParameters()
        {
            // Read Parameters
            ReadOeeMqServerConfigSettings();
            ReadClientMqConfigSettings();
        }

        /// <summary>
        /// Reads OEE MQ parameters from the Config file
        /// </summary>
        private void ReadOeeMqServerConfigSettings()
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
                        // Clear any previous values
                        _oeeMqServerparameters.Clear();

                        foreach (XmlNode node in nodes)
                        {
                            // Add value to the dictionary
                            _oeeMqServerparameters.Add(node.Name, node.InnerText);
                        }
                    }
                    return;
                }
                _asyncClassLogger.Info("File not found: " + _oeeServerConfig, _type.FullName, "ReadMdeMqConfigSettings");
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ReadMdeMqConfigSettings");
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
                    XmlNodeList nodes = doc.SelectNodes(xpath: "StrategyMQParameters/*");
                    if (nodes != null)
                    {
                        // Clear any previous values
                        _clientMqParameters.Clear();

                        foreach (XmlNode node in nodes)
                        {
                            if (node.Name.Equals(Constants.OrderExecutionClientMqParameters.InquiryResponseQueue))
                            {
                                node.InnerText = inquiryQueueId + "_queue";
                            }
                            else if (
                                node.Name.Equals(
                                    Constants.OrderExecutionClientMqParameters.InquiryResponseRoutingKey))
                            {
                                node.InnerText = inquiryQueueId + ".routingkey";
                            }
                            // Add value to the dictionary
                            _clientMqParameters.Add(node.Name, node.InnerText);
                        }
                    }
                    return;
                }
                _asyncClassLogger.Info("File not found: " + _clientConfig, _type.FullName, "ReadClientMqConfigSettings");
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ReadStrategyMqConfigSettings");
            }
        }
    }
}
