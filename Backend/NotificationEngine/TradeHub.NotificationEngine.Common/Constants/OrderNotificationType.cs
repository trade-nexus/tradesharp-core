using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.NotificationEngine.Common.Constants
{
    /// <summary>
    /// Supported types for order notifications
    /// </summary>
    public enum OrderNotificationType
    {
        New,
        Accepted,
        Executed,
        Rejected
    }
}
