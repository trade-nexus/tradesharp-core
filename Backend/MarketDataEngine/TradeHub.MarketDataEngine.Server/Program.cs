using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.MarketDataProvider;
using TradeHub.MarketDataEngine.MarketDataProviderGateway.Utility;
using TradeHub.MarketDataEngine.Server.Service;

namespace TradeHub.MarketDataEngine.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //using (var ctx = ContextRegistry.GetContext())
            {
                ApplicationController applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
                if (applicationController != null) applicationController.StartServer();
                //DataProviderInitializer.GetMarketDataProviderInstance("Blackwood");
            }
            //while (true)
            //{
                
            //}
        }
    }
}
