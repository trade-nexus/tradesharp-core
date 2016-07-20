using System;
using System.Collections.Generic;
using System.Xml;
using TraceSourceLogger;

namespace TradeHub.OrderExecutionProvider.Tradier.Utility
{
    /// <summary>
    /// Read provider paramater from file
    /// </summary>
    public class ParameterReader
    {
        private Type _type = typeof (ParameterReader);
        private string _paramsFileName;
        private Dictionary<string, string> _parameters;

        public ParameterReader(string paramsFileName)
        {
            _parameters=new Dictionary<string, string>();
            _paramsFileName = paramsFileName;
            ReadParamters();
        }

        /// <summary>
        /// Get parameter value
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public string GetParameterValue(string parameterName)
        {
            if (_parameters.ContainsKey(parameterName))
            {
                return _parameters[parameterName];
            }
            return string.Empty;
        }

        /// <summary>
        /// Reads parameters from the configuration file
        /// </summary>
        private void ReadParamters()
        {
            try
            {
                var doc = new XmlDocument();

                // Read configuration file
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + _paramsFileName);

                // Read all the parametes defined in the configuration file
                XmlNodeList configNodes = doc.SelectNodes(xpath: "Tradier/*");
                if (configNodes != null)
                {
                    // Extract individual attribute value
                    foreach (XmlNode node in configNodes)
                    {
                        _parameters.Add(node.Name, node.InnerText);
                    }
                }

                // Log parameters
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(_parameters.ToString(), _type.FullName, "ReadParamters");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadParameters");
            }
        }
    }
}
