using System.Text;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.Common.Core.ValueObjects.MarketData
{
    public class Subscribe : IMarketDataRequest
    {
        // Identifies the Market Data Request Message as "Subscribe"
        public int RequestType{ get { return Constants.MarketData.MarketDataRequest.Subscribe; } }

        // Security for which to subscribe
        public Security Security { get; set; }

        // Unique ID for the request
        public string Id { get; set; }

        // Name of the Data Provider to subscribe from
        public string MarketDataProvider { get; set; }

        /// <summary>
        /// Overrides ToString Method to provide Subscribe Info
        /// </summary>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Subscribe :: ");
            stringBuilder.Append(Security);
            stringBuilder.Append(" | ID: " + Id);
            stringBuilder.Append(" | Market Data Provider: " + MarketDataProvider);
            return stringBuilder.ToString();
        }
    }
}