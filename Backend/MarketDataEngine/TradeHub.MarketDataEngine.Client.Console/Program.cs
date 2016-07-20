using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spring.Context.Support;
using TradeHub.MarketDataEngine.Client.Service;

namespace TradeHub.MarketDataEngine.Client.Console
{
    class Program
    {
       
        static void Main(string[] args)
        {
            Client cl=new Client();
            cl.start();

        }
        
        
    }
}
