using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.PositionEngine.Configuration.Service;
using TradeHub.PositionEngine.ProviderGateway.Service;
using TradeHub.PositionEngine.Service;

namespace TradeHub.PositionEngine.Server
{
    class Program
    {
        static void Main(string[] args)
        {
           ApplicationController applicationController = new ApplicationController(new PositionEngineMqServer("PEMQConfig.xml"),new PositionMessageProcessor());
           applicationController.StartServer();


        }
    }
}
