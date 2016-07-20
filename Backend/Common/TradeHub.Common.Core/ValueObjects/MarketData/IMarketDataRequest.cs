using TradeHub.Common.Core.DomainModels;

namespace TradeHub.Common.Core.ValueObjects.MarketData
{
    public interface IMarketDataRequest
    {
        // Identifies the Market Data Request Message Type
        int RequestType { get; }

        // Security for which to subscribe
        Security Security { get; set; }

        // Unique ID for the request
        string Id { get; set; }

        // Name of the Data Provider to subscribe from
        string MarketDataProvider { get; set; }
    }
}
