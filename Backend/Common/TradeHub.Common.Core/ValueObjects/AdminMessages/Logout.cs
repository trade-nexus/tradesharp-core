using System.Text;
using TradeHub.Common.Core.Constants;

namespace TradeHub.Common.Core.ValueObjects.AdminMessages
{
    /// <summary>
    /// Logout message to disconnect from the gateway
    /// </summary>
    public class Logout : IAdminMessage
    {
        // Identifies the Admin Message Type
        public string AdminMessageType
        {
            get { return Constants.AdminMessageType.Logout; }
        }

        // Name of Market Data Provider
        public string MarketDataProvider { get; set; }
        // Name of Order Execution Provider
        public string OrderExecutionProvider { get; set; }

        /// <summary>
        /// Overrides ToString Method to provide Logout Message Info
        /// </summary>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Logout :: ");
            stringBuilder.Append(" | Market Data Provider: " + MarketDataProvider);
            stringBuilder.Append(" | Order Execution Provider: " + OrderExecutionProvider);
            return stringBuilder.ToString();
        }
    }
}
