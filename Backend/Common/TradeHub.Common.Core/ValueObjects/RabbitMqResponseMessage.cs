using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.ValueObjects
{
    /// <summary>
    /// Indicates the Type of message inside response bytes
    /// </summary>
    public enum MessageType
    {
        Order,
        Execution
    }

    /// <summary>
    /// Contains message which is received using Native RabbitMQ
    /// </summary>
    public class RabbitMqResponseMessage
    {
        public MessageType Type { get; set; }
        public string ReplyTo { get; set; }
        public byte[] Message { get; set; }
    }
}
