using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.ValueObjects
{
    /// <summary>
    /// Object which can be sent over Messaging Queues containing Information in Bytes
    /// </summary>
    public class MessageQueueObject
    {
        public byte[] Message { get; set; }
    }
}
