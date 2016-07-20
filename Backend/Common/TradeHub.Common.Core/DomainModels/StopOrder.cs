

using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// 
    /// </summary>
    public class StopOrder : SimpleOrder
    {
        private StopOrder() : base("")
        {
        }

        public StopOrder(string orderExecutionProivder) : base(orderExecutionProivder)
        {
        }

        public StopOrder(string orderID, string orderSide, int orderSize, string orderTif, string orderCurrency, Security security, string orderExecutionProivder)
            : base(orderID, orderSide, orderSize, orderTif, orderCurrency, security, orderExecutionProivder)
        {
        }
    }
}
