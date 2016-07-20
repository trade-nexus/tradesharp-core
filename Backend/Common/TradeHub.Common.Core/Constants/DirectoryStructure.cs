using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Constants
{
    public static class DirectoryStructure
    {
        // ReSharper disable InconsistentNaming
        public static string BASE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +"\\TradeHub";
        public static string STRATEGY_LOCATION = BASE_PATH + "\\StrategyRunner";
        public static string LOGS_LOCATION = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\TradeSharp Logs";
        public static string CLIENT_LOGS_LOCATION = LOGS_LOCATION + "\\Client";
        public static string MDE_LOGS_LOCATION = LOGS_LOCATION + "\\MarketDataEngine";
        public static string OEE_LOGS_LOCATION = LOGS_LOCATION + "\\OrderExecutionEngine";
        public static string PE_LOGS_LOCATION = LOGS_LOCATION + "\\PositionEngine";
        public static string RE_LOGS_LOCATION = LOGS_LOCATION + "\\ReportingEngine";
        public static string TM_LOGS_LOCATION = LOGS_LOCATION + "\\TradeManager";
        // ReSharper enable InconsistentNaming
    }
}
