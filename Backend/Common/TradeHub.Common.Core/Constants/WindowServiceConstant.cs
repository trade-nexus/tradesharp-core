using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Constants
{
    /// <summary>
    /// Windows service constants names
    /// </summary>
    public static class WindowServiceConstant
    {
        // ReSharper disable InconsistentNaming
        public const string MARKET_DATA_ENGINE_SERVICE_NAME = "TradeHub MarketDataEngine Service";
        public const string ORDER_EXECUTION_ENGINE_SERVICE_NAME = "TradeHub OrderExecutionEngine Service";
        public const string POSITION_ENGINE_SERVICE_NAME = "TradeHub PositionEngine Service";
        public const string TRADE_MANAGER_SERVICE_NAME = "TradeHub Trade Manager Service";
        // ReSharper enable InconsistentNaming
    }
}
