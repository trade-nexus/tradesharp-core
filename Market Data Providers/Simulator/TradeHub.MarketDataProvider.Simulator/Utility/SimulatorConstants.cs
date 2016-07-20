using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.MarketDataProvider.Simulator.Utility
{
    /// <summary>
    /// Contains Constant values related to Simulated Data Feed
    /// </summary>
    public static class SimulatorConstants
    {
        /// <summary>
        /// Contains Message Types which simulator can process
        /// </summary>
        public static class MessageTypes
        {
            public const string Tick = "tick";
            public const string LiveBar = "bar";
            public const string HistoricBar = "historicbar";
        }
    }
}
