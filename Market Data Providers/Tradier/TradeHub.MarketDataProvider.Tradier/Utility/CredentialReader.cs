using System;
using System.Xml;
using TraceSourceLogger;
using TradeHub.MarketDataProvider.Tradier.ValueObject;

namespace TradeHub.MarketDataProvider.Tradier.Utility
{
    /// <summary>
    /// Provides Parameters required to establish the Tradier Connection
    /// </summary>
    public static class CredentialReader
    {
        private static readonly Type _type = typeof(CredentialReader);

        /// <summary>
        /// Reads parameters from the configuration file
        /// </summary>
        public static Credentials ReadCredentials(String paramsFileName)
        {
            try
            {
                // Create new object to hold credential details
                Credentials credentials = new Credentials();

                // Create XML document to read condfiguration
                var doc = new XmlDocument();

                // Read configuration file
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + paramsFileName);

                // Read all the parametes defined in the configuration file
                XmlNodeList configNodes = doc.SelectNodes(xpath: "Tradier/*");
                if (configNodes != null)
                {
                    // Extract individual attribute value
                    foreach (XmlNode node in configNodes)
                    {
                        AddParameters(credentials, node.Name, node.InnerText);
                    }
                }

                // Log parameters
                if(Logger.IsInfoEnabled)
                {
                    Logger.Info(credentials.ToString(), _type.FullName, "ReadCredentials");
                }

                return credentials;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadCredentials");
                return null;
            }
        }

        /// <summary>
        /// Adds the value to the matching Attributes property
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        private static void AddParameters(Credentials credentials, string parameterName, string parameterValue)
        {
            if(Logger.IsDebugEnabled)
            {
                Logger.Debug("Adding attribute :: " + parameterName + ":" + parameterValue, _type.FullName, "AddAttributes");
            }
            
            switch (parameterName.Trim().ToLowerInvariant())
            {
                case "apiurl":
                    credentials.ApiUrl = parameterValue.Trim();
                    break;
                case "accesstoken":
                    credentials.AccessToken = parameterValue.Trim();
                    break;
            }
        }
    }
}
