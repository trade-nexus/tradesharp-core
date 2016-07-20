using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.UserInterface.Common.Value_Objects
{
    /// <summary>
    /// All the components of the TradeHub
    /// </summary>
    public enum TradeHubComponent
    {
        MarketDataEngine,
        OrderExecutionEngine,
        SimulatedExchange,
        PositionEngine,
        StrategyRunner,
        DataDownloader,
        Clerk
    }
}
