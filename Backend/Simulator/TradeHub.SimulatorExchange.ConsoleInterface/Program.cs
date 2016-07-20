using System;
using Spring.Context;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.SimulatedExchange.Common;
using TradeHub.SimulatedExchange.SimulatorControler;

namespace TradeHub.SimulatedExchange.ConsoleInterface
{
    public class Program
    {
        static void Main(string[] args)
        {
            //set logging path
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                              "\\TradeHub Logs\\SimulatedExchange";
            Logger.LogDirectory(path);
            IApplicationContext context = ContextRegistry.GetContext();
            var marketDataControler = (MarketDataControler)context.GetObject("MarketDataControler");
            Console.ReadLine();
            marketDataControler.Disconnect();
        }

    }
}
