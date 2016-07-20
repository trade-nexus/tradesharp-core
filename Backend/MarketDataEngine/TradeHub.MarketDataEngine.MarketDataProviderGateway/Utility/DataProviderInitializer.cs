using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.MarketDataProvider;

namespace TradeHub.MarketDataEngine.MarketDataProviderGateway.Utility
{
    /// <summary>
    /// Initializes the Required Market Data Provider instance
    /// </summary>
    public static class DataProviderInitializer
    {
        private static Type _type = typeof (DataProviderInitializer);

        /// <summary>
        /// Provides the Market Data Provider instance depending on the specified provider
        /// </summary>
        public static IMarketDataProvider GetMarketDataProviderInstance(string providerName)
        {
            try
            {
                var doc = new XmlDocument();

                // Read RabbitMQ configuration file
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\AvailableProviders.xml");

                // Read the specified Node value
                XmlNode providerInfo = doc.SelectSingleNode(xpath: "Providers/" + providerName);

                // Check if the Requested Provider info is available
                if (providerInfo == null)
                {
                    if(Logger.IsInfoEnabled)
                    {
                        Logger.Info("Requested Market Data Provider not available.", _type.FullName, "GetMarketDataProviderInstance");
                    }
                    return null;
                }

                IMarketDataProvider marketDataProvider = ContextRegistry.GetContext()[providerName + "MarketDataProvider"] as IMarketDataProvider;

                return marketDataProvider;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "GetMarketDataProviderInstance");
                return null;
            }
        }
    }
}
