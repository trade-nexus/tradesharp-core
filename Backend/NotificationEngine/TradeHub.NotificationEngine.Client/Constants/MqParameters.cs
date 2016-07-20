using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.NotificationEngine.Client.Constants
{
    /// <summary>
    /// Contains available Messaging Queue Parameters
    /// </summary>
    public static class MqParameters
    {
        /// <summary>
        /// Constants related to Message Queues of 'Notification Engine -  Server'
        /// </summary>
        public static class NotificationEngineServer
        {
            public const string ConnectionString = "ConnectionString";
            public const string Exchange = "Exchange";
            public const string OrderMessageRoutingKey = "OrderMessageRoutingKey";
        }

        /// <summary>
        /// Constants related to Message Queues of 'Notification Engine -  Client'
        /// </summary>
        public static class NotificationEngineClient
        {
            public const string ConnectionString = "ConnectionString";
            public const string Exchange = "Exchange";
        }
    }
}
