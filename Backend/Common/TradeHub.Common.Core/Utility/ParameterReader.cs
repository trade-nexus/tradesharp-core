using System;
using System.Collections.Generic;
using System.Xml;
using TraceSourceLogger;

namespace TradeHub.Common.Core.Utility
{
    /// <summary>
    /// Read provider paramater from file
    /// </summary>
    public static class ParameterReader
    {
        private static Type _type = typeof (ParameterReader);

        /// <summary>
        /// Reads parameters from the configuration file
        /// </summary>
        /// <param name="paramsFileName"></param>
        /// <param name="parentNode"></param>
        public static Dictionary<string, string> ReadParamters(string paramsFileName, string parentNode)
        {
            Dictionary<string, string> parameters= new Dictionary<string, string>();

            try
            {
                var doc = new XmlDocument();

                // Read configuration file
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + paramsFileName);

                // Read all the parametes defined in the configuration file
                XmlNodeList configNodes = doc.SelectNodes(xpath: parentNode+"/*");
                if (configNodes != null)
                {
                    // Extract individual attribute value
                    foreach (XmlNode node in configNodes)
                    {
                        parameters.Add(node.Name, node.InnerText);
                    }
                }

                // Log parameters
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(parameters.ToString(), _type.FullName, "ReadParamters");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadParameters");
            }

            return parameters;
        }
    }
}
