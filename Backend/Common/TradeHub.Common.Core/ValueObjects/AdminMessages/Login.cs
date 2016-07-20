using System.Text;
using TradeHub.Common.Core.Constants;

namespace TradeHub.Common.Core.ValueObjects.AdminMessages
{
    /// <summary>
    /// Login message to connect to the Gateway
    /// </summary>
    public class Login : IAdminMessage
    {
        // Identifies the Admin Message Type
        public  string AdminMessageType
        { 
            get { return Constants.AdminMessageType.Login; } 
        }

        // Name of Market Data Provider
        public string MarketDataProvider { get; set; }

        // Name of Order Execution Provider
        public string OrderExecutionProvider { get; set; }

        /// <summary>
        /// Overrides ToString Method to provide Login Message Info
        /// </summary>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Login :: ");
            stringBuilder.Append(" | Market Data Provider: " + MarketDataProvider);
            stringBuilder.Append(" | Order Execution Provider: " + OrderExecutionProvider);
            return stringBuilder.ToString();
        }
    }
}
