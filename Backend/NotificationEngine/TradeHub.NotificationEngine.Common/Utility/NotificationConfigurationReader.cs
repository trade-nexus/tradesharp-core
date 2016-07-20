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
    public static class NotificationConfigurationReader
    {
        private static Type _type = typeof(MqConfigurationReader);

        /// <summary>
        /// Reads Server MQ parameters from the Config file
        /// </summary>
        /// <param name="parentNode">Parent node name from the config file</param>
        /// <param name="configFile">File name from which to read settings</param>
        public static Dictionary<string, string> ReadEmailConfiguration(string parentNode,string configFile)
        {
            try
            {
                if (File.Exists(configFile))
                {
                    var doc = new XmlDocument();

                    // Read Specified configuration file
                    doc.Load(configFile);

                    // Read the specified Node values
                    XmlNodeList nodes = doc.SelectNodes(xpath: parentNode + "/*");

                    // Create dictionary to hold parameter information
                    Dictionary<string, string> serverMqParameters = new Dictionary<string, string>();

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
                Logger.Info("File not found: " + configFile, _type.FullName, "ReadEmailConfiguration");
                return null;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadEmailConfiguration");
                return null;
            }
        }
    }
}
