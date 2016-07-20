using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.CustomAttributes;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.FactoryMethods;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.StrategyEngine.TradeHub;

namespace OpenAndClosePriceBasedStrategy
{
    /// <summary>
    /// Strategy
    /// </summary>
    [TradeHubAttributes("Open and close price based Strategy", typeof(Strategy))]
    public class Strategy : TradeHubStrategy
    {
        /// <summary>
        /// Symbol to use for startegy execution
        /// </summary>
        private readonly string _symbol;

        /// <summary>
        /// Name of market data provider to be used for accessing live market data
        /// </summary>
        private readonly string _marketDataProvider;

        /// <summary>
        /// Name of order execution provider to be used for order management and executions
        /// </summary>
        private readonly string _orderExecutionProvider;

        /// <summary>
        /// Name of historica data provider to be used for retrieving historical market data
        /// </summary>
        private readonly string _historicalDataProvider;

        /// <summary>
        /// old ask price
        /// </summary>
        private decimal _oldAskPrice = 0;

        private int _orderId = 0;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="symbol">Symbol to use for Strategy Execution.</param>
        /// <param name="marketDataProvider">Name of market data provider to be used for accessing live market data</param>
        /// <param name="orderExecutionProvider">Name of order execution provider to be used for order management and executions</param>
        public Strategy(string symbol, string marketDataProvider, string orderExecutionProvider)
            : base(marketDataProvider, orderExecutionProvider, "")
        {
            _symbol = symbol;
            _marketDataProvider = marketDataProvider;
            _orderExecutionProvider = orderExecutionProvider;
            Console.WriteLine("##########################################CW->Constructor Symbol Value=" + _symbol);
        }

        /// <summary>
        /// Overriden to provides additional funtionality for base class function 'Run()'
        /// </summary>
        protected override void OnRun()
        {
            Console.WriteLine("####################################CW->On Run.");
        }

        /// <summary>
        /// Overriden to provides additional funtionality for base class function 'Stop()'
        /// </summary>
        protected override void OnStop()
        {
        }

        #region Connectivity Notifications

        /// <summary>
        /// Called when Logon is received from Market Data Service
        /// </summary>
        /// <param name="marketDataProvider">Name of the market data provider</param>
        public override void OnMarketDataServiceLogonArrived(string marketDataProvider)
        {
            Console.WriteLine("###############################################CW->On Market Data Service Logon Arrived.");
            RequestTickData("1", new Security() { Symbol = _symbol });
            Console.WriteLine("#############################################CW->Symbol Value Logon=" + _symbol);
            RequestLiveBars("B1", new Security() { Symbol = _symbol }, 2, 1, 1);
        }

        /// <summary>
        /// Called when logon is received from Order Execution Service
        /// </summary>
        /// <param name="orderExecutionProvider">Name of the order execution provider</param>
        public override void OnOrderExecutionServiceLogonArrived(string orderExecutionProvider)
        {
            Console.WriteLine("###############################################CW->On Order Execution Service Logon Arrived.");
        }

        /// <summary>
        /// Called when Logon is received from Historical Data Service
        /// </summary>
        /// <param name="historicalDataProvider">Name of the historical data provider</param>
        public override void OnHistoricalDataServiceLogonArrived(string historicalDataProvider)
        {
        }

        /// <summary>
        /// Called when logout is received from Market Data Service
        /// </summary>
        /// <param name="marketDataProvider">Name of the market data provider</param>
        public override void OnMarketDataServiceLogoutArrived(string marketDataProvider)
        {
            Console.WriteLine("###############################################CW->On Market Data Service Logout Arrived.");
        }

        /// <summary>
        /// Called when logout is received from Order Execution Service
        /// </summary>
        /// <param name="orderExecutionProvider">Name of the order execution provider</param>
        public override void OnOrderExecutionServiceLogoutArrived(string orderExecutionProvider)
        {
            Console.WriteLine("###############################################CW->On Order Execution Service Logout Arrived.");
        }

        /// <summary>
        /// Called when Logout is received from Historical Data Service
        /// </summary>
        /// <param name="marketDataProvider">Name of the historical data provider</param>
        public override void OnHistoricalDataServiceLogoutArrived(string marketDataProvider)
        {
        }

        public override void OnMarketDataServiceConnected()
        {
            base.OnMarketDataServiceConnected();

            // Request Logon
            LogonRequestMarketData(_marketDataProvider);
        }

        public override void OnOrderExecutionServiceConnected()
        {
            base.OnOrderExecutionServiceConnected();

            // Request Logon
            LogonRequestOrderExecution(_orderExecutionProvider);
        }

        #endregion

        #region Request Data

        /// <summary>
        /// Sends request for live tick data
        /// </summary>
        /// <param name="id">Unique ID to distinguish the request</param>
        /// <param name="security">Contains symbol information</param>
        private void RequestTickData(string id, Security security)
        {
            Console.WriteLine("#############################################CW->Symbol Value Tick Data=" + security.Symbol);
            // Get Tick subscription message
            Subscribe subscribe = SubscriptionMessage.TickSubscription(id, security, _marketDataProvider);

            // Send subscription request
            this.Subscribe(subscribe);
        }

        /// <summary>
        /// Sends request to receive live bars
        /// </summary>
        /// <param name="id">Unique ID to distinguish the request</param>
        /// <param name="security">Contains symbol information</param>
        /// <param name="length">Bar length in seconds</param>
        /// <param name="pipSize">Smallest pip size for the bars</param>
        /// <param name="barSeed">Seed value to be used for bar generation</param>
        private void RequestLiveBars(string id, Security security, decimal length, decimal pipSize, decimal barSeed)
        {
            // Create Live Bar request message
            BarDataRequest barDataRequest = SubscriptionMessage.LiveBarSubscription(id, security, BarFormat.TIME,
                BarPriceType.LAST, length, pipSize, barSeed, _marketDataProvider);

            // Send live bar request
            base.Subscribe(barDataRequest);
        }

        /// <summary>
        /// Sends request to retrieve historical bar data
        /// </summary>
        /// <param name="id">Unique ID to distinguish the request</param>
        /// <param name="security">Contains symbol information</param>
        /// <param name="startTime">Time from which to start the historical bar data</param>
        /// <param name="endTime">Time at which the historical bar data should end</param>
        /// <param name="interval">Bar interval</param>
        private void RequestHistoricalBarData(string id, Security security, DateTime startTime, DateTime endTime, uint interval)
        {
            // Create Historical Bar request message
            HistoricDataRequest historicBarDataRequest = SubscriptionMessage.HistoricDataSubscription(id, security,
                startTime, endTime, interval, BarPriceType.LAST, _historicalDataProvider);

            // Send historical bar request
            this.Subscribe(historicBarDataRequest);
        }

        #endregion

        #region Receivce Data

        /// <summary>
        /// Called when new Tick data is received
        /// </summary>
        /// <param name="tick">Contains live tick/quote information</param>
        public override void OnTickArrived(Tick tick)
        {

        }

        /// <summary>
        /// Called when new Live Bar is received
        /// </summary>
        /// <param name="bar">Contains complete bar detail</param>
        public override void OnBarArrived(Bar bar)
        {
            Console.WriteLine("########################################CW->On bar Arrived. bar close = " 
                + bar.Close + " | bar open = " +bar.Open);

            if (bar.Close > bar.Open)
            {
                _orderId = _orderId + 1;
                Console.WriteLine("#######################################CW->Sending Buy Order.");
                SendMarketOrder("M"+ _orderId, new Security() { Symbol = _symbol }, 100, OrderSide.BUY);
            }
            else if (bar.Close < bar.Open)
            {
                _orderId = _orderId + 1;
                Console.WriteLine("#######################################CW->Sending Sell Order.");
                SendMarketOrder("M"+ _orderId, new Security() { Symbol = _symbol }, 100, OrderSide.SELL);
            }
        }

        /// <summary>
        /// Called when Historical Bars are received
        /// </summary>
        /// <param name="historicBarData">Contains all the historical data previously requested</param>
        public override void OnHistoricalDataArrived(HistoricBarData historicBarData)
        {
        }

        #endregion

        #region Send Order

        /// <summary>
        /// Sends a new market order to the server
        /// </summary>
        /// <param name="id">Unique ID to distinguish the order</param>
        /// <param name="security">Contains symbol information</param>
        /// <param name="orderSize">Trade size or lot</param>
        /// <param name="orderSide">Side on which to send the order</param>
        private void SendMarketOrder(string id, Security security, int orderSize, string orderSide = OrderSide.BUY)
        {
            // Create new Market Order
            MarketOrder marketOrder = OrderMessage.GenerateMarketOrder(id, security, orderSide, orderSize, _orderExecutionProvider);

            // Send market order
            this.SendOrder(marketOrder);
        }

        /// <summary>
        /// Sends a new limit order to the server
        /// </summary>
        /// <param name="id">Unique ID to distinguish the order</param>
        /// <param name="security">Contains symbol information</param>
        /// <param name="orderSize">Trade size or lot</param>
        /// <param name="limitPrice">Limit price</param>
        /// <param name="orderSide">Side on which to send the order</param>
        private void SendLimitOrder(string id, Security security, int orderSize, decimal limitPrice, string orderSide = OrderSide.BUY)
        {
            // Create new Limit Order
            LimitOrder limitOrder = OrderMessage.GenerateLimitOrder(id, security, orderSide, orderSize, limitPrice, _orderExecutionProvider);

            // Send limit order
            this.SendOrder(limitOrder);
        }

        /// <summary>
        /// Sends request to cancel an active order
        /// </summary>
        /// <param name="orderId">Unique ID of the order to be cancelled</param>
        private void CancelLimitOrder(string orderId)
        {
            // Send order cancellation request
            this.CancelOrder(orderId);
        }

        #endregion

        #region Order Notifications

        /// <summary>
        /// Called when order is accepted by the exchange
        /// </summary>
        /// <param name="order">Contains order details</param>
        public override void OnNewArrived(Order order)
        {
        }

        /// <summary>
        /// Called when order execution is received 
        /// </summary>
        /// <param name="execution">Contains complete execution details</param>
        public override void OnExecutionArrived(Execution execution)
        {
        }

        /// <summary>
        /// Called when order cancellation is received
        /// </summary>
        /// <param name="order">Contains cancelled order information</param>
        public override void OnCancellationArrived(Order order)
        {
        }

        /// <summary>
        /// Called when order is rejected by the exchange
        /// </summary>
        /// <param name="rejection">Contains rejection details</param>
        public override void OnRejectionArrived(Rejection rejection)
        {
        }

        #endregion
    }
}
