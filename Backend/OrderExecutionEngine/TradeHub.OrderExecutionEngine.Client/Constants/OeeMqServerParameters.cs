using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.OrderExecutionEngine.Client.Constants
{
    /// <summary>
    /// Contains the Parameter names used for communicating with the OEE MQ Server
    /// </summary>
    public static class OeeMqServerParameters
    {
        public const string ConnectionString = "ConnectionString";
        public const string Exchange = "Exchange";
        public const string LoginRoutingKey = "LoginRoutingKey";
        public const string LogoutRoutingKey = "LogoutRoutingKey";
        public const string InquiryRoutingKey = "InquiryRoutingKey";
        public const string AppInfoRoutingKey = "AppInfoRoutingKey";
        public const string HeartbeatRoutingKey = "HeartbeatRoutingKey";
        public const string MarketOrderRoutingKey = "MarketOrderRoutingKey";
        public const string OrderRequestRoutingKey = "OrderRequestRoutingKey";
        public const string LimitOrderRoutingKey = "LimitOrderRoutingKey";
        public const string StopOrderRoutingKey = "StopOrderRoutingKey";
        public const string StopLimitOrderRoutingKey = "StopLimitOrderRoutingKey";
        public const string CancelOrderRoutingKey = "CancelOrderRoutingKey";
        public const string LocateResponseRoutingKey = "LocateResponseRoutingKey";
    }
}
