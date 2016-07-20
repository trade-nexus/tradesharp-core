using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.OrderExecutionProvider;

namespace TradeHub.OrderExecutionEngine.OrderExecutionProviderGateway.Utility
{
    /// <summary>
    /// Initializes the required Order Execution Provider Instance
    /// </summary>
    public static class ExecutionProviderInitializer
    {
        private static Type _type = typeof(ExecutionProviderInitializer);

        /// <summary>
        /// Provides the Order Execution Provider instance depending on the specified provider
        /// </summary>
        public static IOrderExecutionProvider GetOrderExecutionProviderInstance(string providerName)
        {
            try
            {
                var doc = new XmlDocument();

                // Read RabbitMQ configuration file
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\AvailableOEProviders.xml");

                // Read the specified Node value
                XmlNode providerInfo = doc.SelectSingleNode(xpath: "Providers/" + providerName);

                // Check if the Requested Provider info is available
                if (providerInfo == null)
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Requested Order Execution Provider not available.", _type.FullName, "GetOrderExecutionProviderInstance");
                    }
                    return null;
                }

                IOrderExecutionProvider orderExecutionProvider = ContextRegistry.GetContext()[providerName + "OrderExecutionProvider"] as IOrderExecutionProvider;

                return orderExecutionProvider;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "GetOrderExecutionProviderInstance");
                return null;
            }
        }
    }
}
