using System;
using System.Text;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.Common.Core.ValueObjects.MarketData
{
    public class HistoricDataRequest : IMarketDataRequest
    {
        // Identifies the Market Data Request Message as "Historic Data Request"
        public int RequestType { get { return Constants.MarketData.MarketDataRequest.Historic; } }

        private string _id = string.Empty;
        private string _marketDataProvider = string.Empty;
        private DateTime _startTime = default(DateTime);
        private DateTime _endTime = default(DateTime);
        private uint _interval = default(uint);
        private string _barType = string.Empty;

        // Security for which to subscribe
        public Security Security { get; set; }

        // Unique ID for the request
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        // Name of the Data Provider to subscribe from
        public string MarketDataProvider
        {
            get { return _marketDataProvider; }
            set { _marketDataProvider = value; }
        }

        // Starting time value
        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        // End time value
        public DateTime EndTime
        {
            get { return _endTime; }
            set { _endTime = value; }
        }

        // Bar Interval
        public uint Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }

        // Type of Bar to be subscribed
        public string BarType
        {
            get { return _barType; }
            set { _barType = value; }
        }

        /// <summary>
        /// Overrides ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("HistoricDataRequest :: ");
            stringBuilder.Append(" | " + Security);
            stringBuilder.Append(" | ID: " + Id);
            stringBuilder.Append(" | Interval: " + Interval);
            stringBuilder.Append(" | Bar Type: " + BarType);
            stringBuilder.Append(" | Start Time: " + StartTime);
            stringBuilder.Append(" | End Time: " + EndTime);
            stringBuilder.Append(" | Market Data Provider: " + MarketDataProvider);

            return stringBuilder.ToString();
        }
    }
}
