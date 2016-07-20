using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.SimulatedExchange.Common.Interfaces
{
    /// <summary>
    /// Interface to be implemented by class responsible for communication with connecting modules
    /// </summary>
    public interface ICommunicationController
    {
        event Action MarketDataLoginRequest;
        event Action MarketDataLogoutRequest;
        event Action OrderExecutionLoginRequest;
        event Action OrderExecutionLogoutRequest;
        event Action<Subscribe> TickDataRequest;
        event Action<BarDataRequest> BarDataRequest;
        event Action<HistoricDataRequest> HistoricDataRequest;
        event Action<MarketOrder> MarketOrderRequest;
        event Action<LimitOrder> LimitOrderRequest;

        /// <summary>
        /// Starts MQ Server
        /// </summary>
        void Connect();

        /// <summary>
        /// Stop MQ Server
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Publishes Admin Level request response for Market Data
        /// </summary>
        void PublishMarketAdminMessageResponse(string response);

        /// <summary>
        /// Publishes Admin Level request response for Order Executions
        /// </summary>
        void PublishOrderAdminMessageResponse(string response);

        /// <summary>
        /// Publishes Tick Data
        /// </summary>
        void PublishTickData(Tick tick);

        /// <summary>
        /// Publishes Bar Data
        /// </summary>
        void PublishBarData(Bar bar);

        /// <summary>
        /// Publishes Bar Data
        /// </summary>
        void PublishHistoricData(HistoricBarData historicBarData);

        /// <summary>
        /// Publishes New Order status message
        /// </summary>
        void PublishNewOrder(Order order);

        /// <summary>
        /// Publishes Order Rejection
        /// </summary>
        void PublishOrderRejection(Rejection rejection);

        /// <summary>
        /// Publishes Order Executions
        /// </summary>
        void PublishExecutionOrder(Execution execution);
    }
}
