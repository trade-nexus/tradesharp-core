

using System;

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class StopLimitOrder : SimpleOrder
    {
        private StopLimitOrder():base("")
        {
            
        }

        public StopLimitOrder(string orderExecutionProivder) : base(orderExecutionProivder)
        {
        }

        public StopLimitOrder(string orderID, string orderSide, int orderSize, string orderTif, string orderCurrency, Security security, string orderExecutionProivder)
            : base(orderID, orderSide, orderSize, orderTif, orderCurrency, security, orderExecutionProivder)
        {
        }
    }
}
