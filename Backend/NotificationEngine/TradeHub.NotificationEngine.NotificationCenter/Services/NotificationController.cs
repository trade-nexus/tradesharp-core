using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.NotificationEngine.Common.Constants;
using TradeHub.NotificationEngine.Common.ValueObject;
using TradeHub.NotificationEngine.NotificationCenter.Manager;

namespace TradeHub.NotificationEngine.NotificationCenter.Services
{
    // Provides access to all notification's related actions
    public class NotificationController
    {
        private Type _type = typeof (NotificationController);

        /// <summary>
        /// Provides Email notification functionality
        /// </summary>
        private EmailManager _emailManager;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public NotificationController()
        {
            // Initialize objects
            _emailManager = new EmailManager();
        }

        /// <summary>
        /// Handles new incoming Order Notification
        /// </summary>
        /// <param name="notification">Contains notification details specific to Orders</param>
        public void NewNotificationArrived(OrderNotification notification)
        {
            if (notification.NotificationType.Equals(NotificationType.Email))
            {
                _emailManager.SendNotification(notification);
            }
        }
    }
}
