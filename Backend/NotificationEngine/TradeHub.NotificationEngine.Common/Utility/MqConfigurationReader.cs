using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TraceSourceLogger;

namespace TradeHub.NotificationEngine.Common.Utility
{
    /// <summary>
    /// Reads Message Queue's configuration parameters to be used by Notification Engine
    /// </summary>
    public static class MqConfigurationReader
    {
        private static Type _type = typeof (MqConfigurationReader);

        /// <summary>
        /// Reads Server MQ parameters from the Config file
        /// </summary>
        /// <param name="serverConfig">File name from which to read settings</param>
        public static Dictionary<string, string> ReadServerMqProperties(string serverConfig)
        {
            try
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + serverConfig))
                {
                    var doc = new XmlDocument();

                    // Read Specified configuration file
                    doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + serverConfig);

                    // Read the specified Node values
                    XmlNodeList nodes = doc.SelectNodes(xpath: "RabbitMQ/*");

                    // Create dictionary to hold parameter information
                    Dictionary<string, string> serverMqParameters= new Dictionary<string, string>();

                    if (nodes != null)
                    {
                        foreach (XmlNode node in nodes)
                        {
                            // Add value to the dictionary
                            serverMqParameters.Add(node.Name, node.InnerText);
                        }
                    }
                    return serverMqParameters;
                }
                Logger.Info("File not found: " + serverConfig, _type.FullName, "ReadServerMqProperties");
                return null;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadServerMqProperties");
                return null;
            }
        }

        /// <summary>
        /// Reads Client MQ parameters from the Config file
        /// <param name="clientConfig">File name from which to read settings</param>
        /// </summary>
        public static Dictionary<string, string> ReadClientMqProperties(string clientConfig)
        {
            try
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + clientConfig))
                {
                    var doc = new XmlDocument();

                    // Read Specified configuration file
                    doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + clientConfig);

                    // Read the specified Node values
                    XmlNodeList nodes = doc.SelectNodes(xpath: "ClientRabbitMQ/*");

                    // Create dictionary to hold parameter information
                    Dictionary<string, string> clientMqParameters = new Dictionary<string, string>();
                    if (nodes != null)
                    {
                        foreach (XmlNode node in nodes)
                        {
                            // Add value to the dictionary
                            clientMqParameters.Add(node.Name, node.InnerText);
                        }
                    }
                    return clientMqParameters;
                }
                
                Logger.Info("File not found: " + clientConfig, _type.FullName, "ReadClientMqProperties");
                return null;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadClientMqProperties");
                return null;
            }
        }
    }
}
