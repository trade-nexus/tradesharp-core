using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.Common.HistoricalDataProvider.Utility
{
    /// <summary>
    /// Blue print for creating an Order Executor to be used with Backtesting
    /// </summary>
    public interface IOrderExecutor
    {
        event Action<Order> NewOrderArrived;
        event Action<Execution> ExecutionArrived;
        event Action<Order> CancellationArrived;
        event Action<Rejection> RejectionArrived;

        /// <summary>
        /// Called when new tick is recieved
        /// </summary>
        /// <param name="tick">TradeHub Tick</param>
        void TickArrived(Tick tick);

        /// <summary>
        /// Called when new Bar is received
        /// </summary>
        /// <param name="bar">TradeHub Bar</param>
        void BarArrived(Bar bar);

        /// <summary>
        /// Called when new market order is received
        /// </summary>
        /// <param name="marketOrder">TardeHub MarketOrder</param>
        void NewMarketOrderArrived(MarketOrder marketOrder);

        /// <summary>
        /// Called when new limit order is recieved
        /// </summary>
        /// <param name="limitOrder">TradeHub LimitOrder</param>
        void NewLimitOrderArrived(LimitOrder limitOrder);

        /// <summary>
        /// Called when new cancel order request is recieved
        /// </summary>
        /// <param name="cancelOrder"></param>
        void CancelOrderArrived(Order cancelOrder);

        /// <summary>
        /// Clear necessary resources
        /// </summary>
        void Clear();
    }
}
