using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spring.Context.Support;
using TradeHub.TradeManager.Server.Service;

namespace TradeHub.TradeManager.Server.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize Object
            ApplicationController applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;

            // Start Server communicator
            if (applicationController != null) 
                applicationController.StartCommunicator();
        }
    }
}
