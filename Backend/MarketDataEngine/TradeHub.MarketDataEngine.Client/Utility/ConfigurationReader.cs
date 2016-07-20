using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TraceSourceLogger;

namespace TradeHub.MarketDataEngine.Client.Utility
{
    /// <summary>
    /// Reads required parameters from the specified Config file
    /// </summary>
    public class ConfigurationReader
    {
        private Type _type = typeof(ConfigurationReader);
     
        // Name of MDE Server Configuration File
        private readonly string _mdeServerConfig;
        // Name of Strategy Gateway Configuration File
        private readonly string _clientConfig;

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        private Dictionary<string, string> _mdeMqServerparameters;

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        private Dictionary<string, string> _clientMqParameters;

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        public Dictionary<string, string> MdeMqServerparameters
        {
            get { return _mdeMqServerparameters; }
            set { _mdeMqServerparameters = value; }
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
        public ConfigurationReader(string mdeServerConfig, string clientConfig)
        {
            _mdeServerConfig = mdeServerConfig;
            _clientConfig = clientConfig;

            // Initialize values
            _clientMqParameters = new Dictionary<string, string>();
            _mdeMqServerparameters = new Dictionary<string, string>();
        }

        /// <summary>
        /// Reads configuration parameters
        /// </summary>
        public void ReadParameters()
        {
            // Read Parameters
            ReadMdeMqServerConfigSettings();
            ReadClientMqConfigSettings();
        }

        /// <summary>
        /// Reads MDE MQ parameters from the Config file
        /// </summary>
        private void ReadMdeMqServerConfigSettings()
        {
            try
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + _mdeServerConfig))
                {
                    var doc = new XmlDocument();

                    // Read Specified configuration file
                    doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + _mdeServerConfig);

                    // Read the specified Node values
                    XmlNodeList nodes = doc.SelectNodes(xpath: "RabbitMQ/*");
                    if (nodes != null)
                    {
                        // Clear previous data
                        _mdeMqServerparameters.Clear();

                        foreach (XmlNode node in nodes)
                        {
                            // Add value to the dictionary
                            _mdeMqServerparameters.Add(node.Name, node.InnerText);
                        }
                    }
                }
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
                    XmlNodeList nodes = doc.SelectNodes(xpath: "StrategyMQParameters/*");
                    if (nodes != null)
                    {
                        // Clear previous data
                        _clientMqParameters.Clear();

                        foreach (XmlNode node in nodes)
                        {
                            if (node.Name.Equals(Constants.ClientMqParameterNames.InquiryResponseQueue))
                            {
                                node.InnerText = inquiryQueueId + "_queue";
                            }
                            else if (node.Name.Equals(Constants.ClientMqParameterNames.InquiryResponseRoutingKey))
                            {
                                node.InnerText = inquiryQueueId + ".routingkey";
                            }
                            // Add value to the dictionary
                            _clientMqParameters.Add(node.Name, node.InnerText);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadStrategyMqConfigSettings");
            }
        }
    }
}
