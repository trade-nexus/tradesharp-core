using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.NotificationEngine.Common.ValueObject;

namespace TradeHub.NotificationEngine.Client.Service
{
    /// <summary>
    /// Blue print to be used for communciating with the Server
    /// </summary>
    public interface ICommunicator
    {
        /// <summary>
        /// Indicates if the communication medium is open or not
        /// </summary>
        /// <returns></returns>
        bool IsConnected();

        /// <summary>
        /// Opens necessary connections to start 
        /// </summary>
        void Connect();

        /// <summary>
        /// Closes communication channels
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Forwards order notifications to Server
        /// </summary>
        void SendNotification(OrderNotification notification);
    }
}
