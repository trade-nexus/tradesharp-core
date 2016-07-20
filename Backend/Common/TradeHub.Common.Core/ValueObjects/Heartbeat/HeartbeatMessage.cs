using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.ValueObjects.Heartbeat
{
    /// <summary>
    /// Contains Heartbeat Info
    /// </summary>
    public class HeartbeatMessage
    {
        /// <summary>
        /// Application ID which generated the Heartbeat Message
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Time duration between expected Heartbeat
        /// </summary>
        public int HeartbeatInterval { get; set; }

        /// <summary>
        /// Routing Key for Replying back to the sender Application
        /// </summary>
        public string ReplyTo { get; set; }

        /// <summary>
        /// Overrides ToString method to proived Heartbeat Message Info
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("HeartbeatMessage :: ");
            stringBuilder.Append(" Application ID: " + ApplicationId);
            stringBuilder.Append(" | Heartbeat Interval: " + HeartbeatInterval);
            stringBuilder.Append(" | Reply To: " + ReplyTo);
            
            return stringBuilder.ToString();
        }
    }
}
