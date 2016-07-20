using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spring.Context.Support;
using TradeHub.MarketDataEngine.Server.Service;

namespace TradeHub.MarketDataEngine.Server.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            ApplicationController applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
            if (applicationController != null) applicationController.StartServer();
        }
    }
}
