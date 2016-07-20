using System.Text;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.Common.Core.ValueObjects.MarketData
{
    public class BarDataRequest : IMarketDataRequest
    {
        // Identifies the Market Data Request Message as "Bar Data Request"
        public int RequestType { get { return Constants.MarketData.MarketDataRequest.BarData; } }
     
        // Security for which to subscribe
        public Security Security { get; set; }

        // Unique ID for the request
        public string Id { get; set; }

        // Name of the Data Provider to subscribe from
        public string MarketDataProvider { get; set; }
        
        // Format on which to generate bars
        public string BarFormat { get; set; }

        // Lenght of required Bar
        public decimal BarLength { get;set; }

        // Bar Pip Size
        public decimal PipSize { get; set; }

        // Bar Seed
        public decimal BarSeed { get; set; }

        // Price Type to be used for generating Bars
        public string BarPriceType { get; set; }

        /// <summary>
        /// Overrides ToString Method to provide Bar Data Request Info
        /// </summary>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("BarDataRequest :: ");
            stringBuilder.Append(Security);
            stringBuilder.Append(" | ID: " + Id);
            stringBuilder.Append(" | Bar Format: " + BarFormat);
            stringBuilder.Append(" | Bar Length: " + BarLength);
            stringBuilder.Append(" | Pip Size: " + PipSize);
            stringBuilder.Append(" | Bar Seed: " +  BarSeed);
            stringBuilder.Append(" | Market Data Provider: " + MarketDataProvider);
            return stringBuilder.ToString();
        }
    }
}