using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.Common.Core.OrderExecutionProvider
{
    /// <summary>
    /// Interface to be implemented by Order Execution Providers which give Limit Order functionality 
    /// </summary>
    public interface ILimitOrderProvider : IOrderExecutionProvider
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
        /// Raised when Order is accepted by the Order Execution Provider Gateway
        /// </summary>
        event Action<Order> CancellationArrived;

        /// <summary>
        /// Raised when Order is rejected by the Order Execution Provider Gateway
        /// </summary>
        event Action<Rejection> RejectionArrived;

        /// <summary>
        /// Sends Limit Order on the given Order Execution Provider
        /// </summary>
        /// <param name="limitOrder">TradeHub LimitOrder</param>
        void SendLimitOrder(LimitOrder limitOrder);

        /// <summary>
        /// Sends Limit Order Cancallation on the given Order Execution Provider
        /// </summary>
        /// <param name="order">TradeHub Order</param>
        void CancelLimitOrder(Order order);
    }
}
