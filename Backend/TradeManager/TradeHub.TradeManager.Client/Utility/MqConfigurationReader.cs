using System;
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
