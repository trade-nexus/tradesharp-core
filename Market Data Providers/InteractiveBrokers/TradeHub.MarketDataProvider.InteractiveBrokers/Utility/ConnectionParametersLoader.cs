using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TraceSourceLogger;
using TradeHub.MarketDataProvider.InteractiveBrokers.ValueObjects;

namespace TradeHub.MarketDataProvider.InteractiveBrokers.Utility
{
    /// <summary>
    /// Provides Parameters required to establish the IB Connection
    /// </summary>
    public class ConnectionParametersLoader
    {
        private readonly Type _type = typeof(ConnectionParametersLoader);

        private readonly String _paramsFileName;
        private ConnectionParameters _parameters;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="paramsFileName"></param>
        public ConnectionParametersLoader(String paramsFileName)
        {
            _paramsFileName = paramsFileName;
            _parameters = new ConnectionParameters();
            ReadParamters();
        }

        /// <summary>
        /// Parameters required for Connection
        /// </summary>
        public ConnectionParameters Parameters
        {
            get { return _parameters; }
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
                XmlNodeList configNodes = doc.SelectNodes(xpath: "InteractiveBrokers/*");
                if (configNodes != null)
                {
                    // Extract individual attribute value
                    foreach (XmlNode node in configNodes)
                    {
                        AddParameters(node.Name, node.InnerText);
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
                _parameters = null;
                Logger.Error(exception, _type.FullName, "ReadParameters");
            }
        }

        /// <summary>
        /// Adds the value to the matching Attributes property
        /// </summary>
        /// <param name="parameterName">Name of Selected Parameter</param>
        /// <param name="parameterValue">Value of given Parameter</param>
        private void AddParameters(string parameterName, string parameterValue)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Adding attribute :: " + parameterName + ":" + parameterValue, _type.FullName, "AddAttributes");
            }

            switch (parameterName.Trim().ToLowerInvariant())
            {
                case "host":
                    _parameters.Host = parameterValue.Trim();
                    break;
                case "port":
                    _parameters.Port = Convert.ToInt32(parameterValue.Trim());
                    break;
                case "clientid":
                    _parameters.ClientId = Convert.ToInt32(parameterValue.Trim());
                    break;
            }
        }
    }
}
