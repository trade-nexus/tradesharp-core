using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.ValueObjects
{
    /// <summary>
    /// Contains message with is routing info to be published using Native RabbitMQ
    /// </summary>
    public class RabbitMqRequestMessage
    {
        public string RequestTo { get; set; }
        public byte[] Message { get; set; }
    }
}
