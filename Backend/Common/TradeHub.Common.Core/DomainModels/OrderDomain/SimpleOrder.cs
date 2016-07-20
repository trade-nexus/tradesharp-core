

using System;

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    /// <summary>
    /// An Order that can be sent directly to the market
    /// </summary>
    [Serializable]
    public class SimpleOrder : Order
    {
        public SimpleOrder(string orderExecutionProivder) : base(orderExecutionProivder)
        {
        }

        public SimpleOrder(string orderID, string orderSide, int orderSize, string orderTif, string orderCurrency, Security security, string orderExecutionProivder)
            : base(orderID, orderSide, orderSize, orderTif, orderCurrency, security, orderExecutionProivder)
        {
        }
    }
}
