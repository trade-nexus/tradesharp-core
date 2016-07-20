using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.OrderExecutionEngine.Client.Constants
{
    /// <summary>
    /// Contains the Parameter names used for getting response back from OEE MQ Server
    /// </summary>
    public class OrderExecutionClientMqParameters
    {
        public const string ConnectionString = "ConnectionString";
        public const string Exchange = "Exchange";
        public const string AdminMessageQueue = "AdminMessageQueue";
        public const string AdminMessageRoutingKey = "AdminMessageRoutingKey";
        public const string OrderMessageQueue = "OrderMessageQueue";
        public const string OrderMessageRoutingKey = "OrderMessageRoutingKey";
        public const string ExecutionMessageQueue = "ExecutionMessageQueue";
        public const string ExecutionMessageRoutingKey = "ExecutionMessageRoutingKey";
        public const string RejectionMessageQueue = "RejectionMessageQueue";
        public const string RejectionMessageRoutingKey = "RejectionMessageRoutingKey";
        public const string InquiryResponseQueue = "InquiryResponseQueue";
        public const string InquiryResponseRoutingKey = "InquiryResponseRoutingKey";
        public const string HeartbeatResponseQueue = "HeartbeatResponseQueue";
        public const string HeartbeatResponseRoutingKey = "HeartbeatResponseRoutingKey";
        public const string LocateMessageQueue = "LocateMessageQueue";
        public const string LocateMessageRoutingKey = "LocateMessageRoutingKey";
    }
}
