

using System;
using System.Text;

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class LimitOrder : Order
    {
        private decimal _limitPrice = default(decimal);

        private LimitOrder()
            : base("")
        {

        }

        public LimitOrder(string orderExecutionProivder) : base(orderExecutionProivder)
        {
        }

        public LimitOrder(string orderID, string orderSide, int orderSize, string orderTif, string orderCurrency, Security security, string orderExecutionProivder)
            : base(orderID, orderSide, orderSize, orderTif, orderCurrency, security, orderExecutionProivder)
        {
        }

        /// <summary>
        /// Gets/Sets Order Limit Price
        /// </summary>
        public decimal LimitPrice
        {
            get { return _limitPrice; }
            set { _limitPrice = value; }
        }

        /// <summary>
        /// Overrider ToString for Order
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Order :: ");
            stringBuilder.Append(Security);
            stringBuilder.Append(" | Order ID: " + OrderID);
            stringBuilder.Append(" | Broker ID: " + BrokerOrderID);
            stringBuilder.Append(" | Side: " + OrderSide);
            stringBuilder.Append(" | Size: " + OrderSize);
            stringBuilder.Append(" | Limit Price: " + LimitPrice);
            stringBuilder.Append(" | Trigger Price: " + TriggerPrice);
            stringBuilder.Append(" | Slippage: " + Slippage);
            stringBuilder.Append(" | TIF: " + OrderTif);
            stringBuilder.Append(" | Status: " + OrderStatus);
            stringBuilder.Append(" | Currency: " + OrderCurrency);
            stringBuilder.Append(" | Exchange: " + Exchange);
            stringBuilder.Append(" | Date Time: " + OrderDateTime);

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Creates a string which is to be published and converted back to LimitOrder on receiver end
        /// </summary>
        public string DataToPublish()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Limit");
            stringBuilder.Append("," + OrderID);
            stringBuilder.Append("," + OrderSide);
            stringBuilder.Append("," + OrderSize); 
            stringBuilder.Append("," + LimitPrice);
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
