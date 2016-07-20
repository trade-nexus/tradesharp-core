using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.MarketDataProvider.Tradier.ValueObject
{
    /// <summary>
    /// Contains individual JSON elements received through streaming quotes
    /// </summary>
    public class QuoteStream
    {
        public string type { get; set; }
        public string symbol { get; set; }
        public string exch { get; set; }
        public string price { get; set; }
        public string size { get; set; }
        public string cvol { get; set; }
        public string date { get; set; }
        public string bid { get; set; }
        public string bidsz { get; set; }
        public string bidexch { get; set; }
        public string biddate { get; set; }
        public string ask { get; set; }
        public string asksz { get; set; }
        public string askexch { get; set; }
        public string askdate { get; set; }
        public string open { get; set; }
        public string high { get; set; }
        public string low { get; set; }
        public string prevClose { get; set; }
    }
}
