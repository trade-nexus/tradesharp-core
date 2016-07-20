using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.TradeManager.Client.Constants
{
    /// <summary>
    /// Contains available Messaging Queue Parameters
    /// </summary>
    public static class MqParameters
    {
        /// <summary>
        /// Constants related to Message Queues of 'Trade Manager -  Server'
        /// </summary>
        public static class TradeManagerServer
        {
            public const string ConnectionString = "ConnectionString";
            public const string Exchange = "Exchange";
            public const string ExecutionMessageRoutingKey = "ExecutionMessageRoutingKey";
        }

        /// <summary>
        /// Constants related to Message Queues of 'Trade Manager -  Client'
        /// </summary>
        public static class TradeManagerClient
        {
            public const string ConnectionString = "ConnectionString";
            public const string Exchange = "Exchange";
        }
    }
}
