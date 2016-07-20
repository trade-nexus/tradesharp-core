using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.MarketDataEngine.Configuration.Service;

namespace TradeHub.MarketDataEngine.Configuration.Tests
{
    class Program
    {
        public static void Main()
        {
            MqServer mqServer=new MqServer("RabbitMq.xml",6000,12000);
            

        }
    }
}
