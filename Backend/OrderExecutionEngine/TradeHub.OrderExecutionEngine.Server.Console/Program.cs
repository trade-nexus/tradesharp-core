using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.OrderExecutionEngine.Server.Service;

namespace TradeHubOrder.OrderExecutionEngine.Server.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) +
                              "\\TradeHub Logs\\OrderExecutionEngine";

            //Logger.LogDirectory(path);
            Logger.Info(path, "", "");
            ApplicationController applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
            if (applicationController != null) applicationController.StartServer();
        }
    }
}
