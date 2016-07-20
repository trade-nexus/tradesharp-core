using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeHub.Common.Core.Constants;
using TradeHub.PositionEngine.Client.Service;

namespace TradeHub.PositionEngine.Client
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            PositionEngineClient positionEngineClient=new PositionEngineClient();
            positionEngineClient.Initialize(OrderExecutionProvider.Simulated);
           
            
            
        }
    
    }
}
