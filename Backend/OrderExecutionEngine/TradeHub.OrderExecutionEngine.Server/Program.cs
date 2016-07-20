using System;
using System.Xml;
using Spring.Context.Support;
using TradeHub.OrderExecutionEngine.Server.Service;

namespace TradeHub.OrderExecutionEngine.Server
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
