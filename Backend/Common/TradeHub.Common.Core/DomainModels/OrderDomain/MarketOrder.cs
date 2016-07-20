

using System;
using System.Text;

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class MarketOrder : Order
    {
        private MarketOrder():base("")
        {
            
        }

        public MarketOrder(string orderExecutionProivder) : base(orderExecutionProivder)
        {
        }

        public MarketOrder(string orderID, string orderSide, int orderSize, string orderTif, string orderCurrency, Security security, string orderExecutionProivder)
            : base(orderID, orderSide, orderSize, orderTif, orderCurrency, security, orderExecutionProivder)
        {
        }
        
        /// <summary>
        /// Creates a string which is to be published and converted back to MarketOrder on receiver end
        /// </summary>
        public string DataToPublish()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Market");
            stringBuilder.Append("," + OrderID);
            stringBuilder.Append("," + OrderSide);
            stringBuilder.Append("," + OrderSize);
            stringBuilder.Append("," + OrderTif);
            stringBuilder.Append("," + Security.Symbol);
            stringBuilder.Append("," + OrderDateTime.ToString("M/d/yyyy h:mm:ss.fff tt"));
            stringBuilder.Append("," + OrderExecutionProvider);
            stringBuilder.Append("," + TriggerPrice);
            stringBuilder.Append("," + Slippage);
            stringBuilder.Append("," + Remarks);
            stringBuilder.Append("," + Exchange);

            return stringBuilder.ToString();
        }
    }
}
