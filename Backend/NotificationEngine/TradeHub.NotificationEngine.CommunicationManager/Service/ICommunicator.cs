using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.NotificationEngine.Common.ValueObject;

namespace TradeHub.NotificationEngine.CommunicationManager.Service
{
    /// <summary>
    /// Blue print for the communication class to be used
    /// </summary>
    public interface ICommunicator
    {
        /// <summary>
        /// Raised when new Order Notificaiton Message is received
        /// </summary>
        event Action<OrderNotification> OrderNotificationEvent;

        /// <summary>
        /// Checks if the medium is available for communication or not
        /// </summary>
        bool IsConnected();

        /// <summary>
        /// Connect necessary services to start communication
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnect necessary services to stop communication
        /// </summary>
        void Disconnect();
    }
}
