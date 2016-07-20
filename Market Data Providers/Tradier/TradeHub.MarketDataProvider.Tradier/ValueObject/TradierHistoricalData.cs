using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.MarketDataProvider.Tradier.ValueObject
{
    /// <summary>
    /// Root Object for JSON data returned from Tradier
    /// </summary>
    class TradierHistoricalData
    {
        public HistoricalDataCollection history { get; set; }
    }

    /// <summary>
    /// Contains collection of data array inside JSON object
    /// </summary>
    public class HistoricalDataCollection
    {
        public List<HistoricalDataDetail> day { get; set; }
    }

    /// <summary>
    /// Contains individual array collection element details
    /// </summary>
    public class HistoricalDataDetail
    {
        public string date { get; set; }
        public double open { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double close { get; set; }
        public int volume { get; set; }
    }
}
