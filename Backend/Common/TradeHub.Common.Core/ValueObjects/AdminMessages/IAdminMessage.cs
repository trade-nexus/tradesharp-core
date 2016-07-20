using TradeHub.Common.Core.Constants;

namespace TradeHub.Common.Core.ValueObjects.AdminMessages
{
    public interface IAdminMessage
    {
        // Identifies the Admin Message Type
        string AdminMessageType { get; }
        
        // Name of Market Data Provider
        string MarketDataProvider { get; set; }

        // Name of Order Execution Provider
        string OrderExecutionProvider { get; set; }
    }
}
