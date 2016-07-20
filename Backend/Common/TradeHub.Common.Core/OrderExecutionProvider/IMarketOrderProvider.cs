using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.Common.Core.OrderExecutionProvider
{
    /// <summary>
    /// Interface to be implemented by Order Execution Providers which give Market Order functionality 
    /// </summary>
    public interface IMarketOrderProvider : IOrderExecutionProvider
    {
        /// <summary>
        /// Raised when Order is accepted by the Order Execution Provider Gateway
        /// </summary>
        event Action<Order> NewArrived; 

        /// <summary>
        /// Raised when Order is Filled (Partial/Full Fill) by the Order Execution Provider Gateway
        /// </summary>
        event Action<Execution> ExecutionArrived;

        /// <summary>
        /// Raised when Order is rejected by the Order Execution Provider Gateway
        /// </summary>
        event Action<Rejection> RejectionArrived; 

        /// <summary>
        /// Sends Market Order on the given Order Execution Provider
        /// </summary>
        /// <param name="marketOrder">TradeHub MarketOrder</param>
        void SendMarketOrder(MarketOrder marketOrder);
    }
}
