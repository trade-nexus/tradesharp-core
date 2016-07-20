using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.MarketDataEngine.Client.Constants
{
    /// <summary>
    /// Contains the Parameter names used for getting response back from the MDE MQ Server
    /// </summary>
    public class ClientMqParameterNames
    {
        public const string ConnectionString = "ConnectionString";
        public const string Exchange = "Exchange";
        public const string AdminMessageQueue = "AdminMessageQueue";
        public const string AdminMessageRoutingKey = "AdminMessageRoutingKey";
        public const string TickDataQueue = "TickDataQueue";
        public const string TickDataRoutingKey = "TickDataRoutingKey";
        public const string HistoricBarDataQueue = "HistoricBarDataQueue";
        public const string HistoricBarDataRoutingKey = "HistoricBarDataRoutingKey";
        public const string LiveBarDataQueue = "LiveBarDataQueue";
        public const string LiveBarDataRoutingKey = "LiveBarDataRoutingKey";
        public const string InquiryResponseQueue = "InquiryResponseQueue";
        public const string InquiryResponseRoutingKey = "InquiryResponseRoutingKey";
        public const string HeartbeatResponseQueue = "HeartbeatResponseQueue";
        public const string HeartbeatResponseRoutingKey = "HeartbeatResponseRoutingKey";
    }
}
