
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.MarketDataEngine.Client.Constants
{
    /// <summary>
    /// Contains the Parameter names used for communicating with the MDE MQ Server
    /// </summary>
    public static class MdeMqServerParameterNames
    {
        public const string ConnectionString = "ConnectionString";
        public const string Exchange = "Exchange";
        public const string SubscribeRoutingKey = "SubscribeRoutingKey";
        public const string UnsubscribeRoutingKey = "UnsubscribeRoutingKey";
        public const string HistoricBarDataRoutingKey = "HistoricBarDataRoutingKey";
        public const string LoginRoutingKey = "LoginRoutingKey";
        public const string LogoutRoutingKey = "LogoutRoutingKey";
        public const string LiveBarSubscribeRoutingKey = "LiveBarSubscribeRoutingKey";
        public const string LiveBarUnsubscribeRoutingKey = "LiveBarUnsubscribeRoutingKey";
        public const string InquiryRoutingKey = "InquiryRoutingKey";
        public const string AppInfoRoutingKey = "AppInfoRoutingKey";
        public const string HeartbeatRoutingKey = "HeartbeatRoutingKey";
    }
}
