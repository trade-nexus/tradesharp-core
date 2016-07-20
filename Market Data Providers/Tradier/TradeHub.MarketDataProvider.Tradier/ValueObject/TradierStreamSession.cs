using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.MarketDataProvider.Tradier.ValueObject
{
    /// <summary>
    /// Contains Stream Session details from the JSON message
    /// </summary>
    public class TradierStreamSession
    {
        public string Sessionid { get; set; }
        public string Url { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
