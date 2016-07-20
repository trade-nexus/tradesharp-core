using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using RestSharp;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.OrderExecutionProvider;
using TradeHub.OrderExecutionProvider.Tradier.Utility;

namespace TradeHub.OrderExecutionProvider.Tradier.Provider
{
    /// <summary>
    /// Tradier order execution provider
    /// </summary>
    public class TradierOrderExecutionProvider : IMarketOrderProvider, ILimitOrderProvider
    {
        private Type _type = typeof (TradierOrderExecutionProvider);
        private string _provider = "Tradier";
        private TradierManager _tradierManager;
        private ConcurrentDictionary<string, Order> _ordersMap;
        private AsyncClassLogger _logger;
        private Timer _timer;
        private ParameterReader _parameterReader;
        
        public TradierOrderExecutionProvider()
        {
            _parameterReader=new ParameterReader("TradierOrderParams.xml");
            _tradierManager = new TradierManager(_parameterReader.GetParameterValue("Account"),
                _parameterReader.GetParameterValue("Token"), _parameterReader.GetParameterValue("BaseUrl"));
            InitializeTimer();
            InitializeLogger();
            _ordersMap=new ConcurrentDictionary<string, Order>();
        }

        /// <summary>
        /// Initialize timer
        /// </summary>
        private void InitializeTimer()
        {
            _timer = new Timer(double.Parse(_parameterReader.GetParameterValue("TimerInterval")));
            //_timer.Elapsed += TimerExpired;
            //_timer.Start();
        }

        /// <summary>
        /// Timer expired
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerExpired(object sender, ElapsedEventArgs e)
        {
            foreach (var order in _ordersMap.Values.ToArray())
            {
                try
                {
                    var orderStatus = _tradierManager.GetOrderStatus(order.BrokerOrderID);
                    if (_logger.IsInfoEnabled)
                    {
                        _logger.Info(orderStatus.order.ToString(), _type.FullName, "TimerExpired");
                    }
                    if (orderStatus.order.status.Equals("filled",StringComparison.InvariantCultureIgnoreCase))
                    {
                        Fill fill = new Fill(new Security() {Symbol = order.Security.Symbol},
                            order.OrderExecutionProvider, order.OrderID);
                        fill.ExecutionId = Guid.NewGuid().ToString();
                        fill.ExecutionType = ExecutionType.Fill;
                        fill.LeavesQuantity = orderStatus.order.remaining_quantity;
                        fill.CummalativeQuantity = orderStatus.order.exec_quantity;
                        fill.ExecutionSize = orderStatus.order.last_fill_quantity;
                        fill.AverageExecutionPrice = orderStatus.order.avg_fill_price;
                        fill.ExecutionPrice = orderStatus.order.last_fill_price;
                        DateTime executionDateTime;
                        if (!DateTime.TryParse(orderStatus.order.transaction_date, out executionDateTime))
                        {
                            executionDateTime = DateTime.UtcNow;
                        }
                        fill.ExecutionDateTime = executionDateTime;
                        fill.ExecutionSide = order.OrderSide;
                        order.OrderStatus = OrderStatus.EXECUTED;
                        Order orderClone = (Order) order.Clone();
                        Execution execution = new Execution(fill, orderClone);
                        execution.OrderExecutionProvider = _provider;
                        if (ExecutionArrived != null)
                        {
                            ExecutionArrived(execution);
                        }
                        Order deleteOrder;
                        //remove order from the map
                        _ordersMap.TryRemove(order.OrderID, out deleteOrder);
                    }
                    else if (orderStatus.order.status.Equals("partially_filled",StringComparison.InvariantCultureIgnoreCase) && order.OrderStatus!=OrderStatus.PARTIALLY_EXECUTED)
                    {
                        Fill fill = new Fill(new Security() { Symbol = order.Security.Symbol },
                            order.OrderExecutionProvider, order.OrderID);
                        fill.ExecutionId = Guid.NewGuid().ToString();
                        fill.ExecutionType = ExecutionType.Partial;
                        fill.LeavesQuantity = orderStatus.order.remaining_quantity;
                        fill.CummalativeQuantity = orderStatus.order.exec_quantity;
                        fill.ExecutionSize = orderStatus.order.last_fill_quantity;
                        fill.AverageExecutionPrice = orderStatus.order.avg_fill_price;
                        fill.ExecutionPrice = orderStatus.order.last_fill_price;
                        DateTime executionDateTime;
                        if (!DateTime.TryParse(orderStatus.order.transaction_date, out executionDateTime))
                        {
                            executionDateTime = DateTime.UtcNow;
                        }
                        fill.ExecutionDateTime = executionDateTime;
                        fill.ExecutionSide = order.OrderSide;
                        order.OrderStatus = OrderStatus.PARTIALLY_EXECUTED;
                        Order orderClone = (Order)order.Clone();
                        Execution execution = new Execution(fill, orderClone);
                        execution.OrderExecutionProvider = _provider;
                        if (ExecutionArrived != null)
                        {
                            ExecutionArrived(execution);
                        }
                    }
                    else if (orderStatus.order.status == "submitted" && order.OrderStatus != OrderStatus.SUBMITTED)
                    {
                        order.OrderStatus = OrderStatus.SUBMITTED;
                        if (NewArrived != null)
                        {
                            NewArrived((Order) order.Clone());
                        }
                    }
                    else if (orderStatus.order.status == "rejected")
                    {
                        if (OrderRejectionArrived != null)
                        {
                            OrderRejectionArrived(new Rejection(order.Security, _provider) { OrderId = order.OrderID });
                        }
                        Order deleteOrder;
                        //remove order from the map
                        _ordersMap.TryRemove(order.OrderID, out deleteOrder);
                    }
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, _type.FullName, "TimerExpired");
                }
            }
        }

        /// <summary>
        /// Initialize Logger and its location
        /// </summary>
        private void InitializeLogger()
        {
            _logger = new AsyncClassLogger("TradierExecutionProvider");
            _logger.SetLoggingLevel();
            _logger.LogDirectory(DirectoryStructure.OEE_LOGS_LOCATION);
        }

        #region Provider Interfaces Implementations

        public event Action<Order> NewArrived;
        public event Action<Execution> ExecutionArrived;
        public event Action<Rejection> RejectionArrived;
        public event Action<string> LogonArrived;
        public event Action<string> LogoutArrived;
        public event Action<Rejection> OrderRejectionArrived;
        public event Action<LimitOrder> OnLocateMessage;
        public event Action<Position> OnPositionMessage;
        public event Action<Order> CancellationArrived;

        /// <summary>
        /// Send Market Order To Tradier
        /// </summary>
        /// <param name="marketOrder"></param>
        public void SendMarketOrder(MarketOrder marketOrder)
        {
            try
            {
                string orderTiff = OrderTif.DAY;
                if (!string.IsNullOrEmpty(marketOrder.OrderTif))
                {
                    orderTiff = marketOrder.OrderTif;
                }
                var orderId = _tradierManager.SendMarketOrder(marketOrder.OrderSide, marketOrder.OrderSize,
                    marketOrder.Security.Symbol, orderTiff);
                marketOrder.BrokerOrderID = orderId;
                _ordersMap.TryAdd(marketOrder.OrderID, marketOrder);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "SendMarketOrder");
            }
        }

        public bool Start()
        {
            if (_timer != null)
            {
                _timer.Elapsed -= TimerExpired;
                _timer.Elapsed += TimerExpired;
                _timer.Start();
            }
            _ordersMap.Clear();
            if (_tradierManager.GetAccountBalance() == HttpStatusCode.OK)
            {
                if (LogonArrived != null)
                {
                    LogonArrived(_provider);
                }
            }
            return true;
        }

        public bool Stop()
        {
            if (_timer != null)
            {
                _timer.Elapsed -= TimerExpired;
                _timer.Stop();
            }
            if (LogoutArrived != null)
            {
                LogoutArrived(_provider);
            }
            return true;
        }

        public bool IsConnected()
        {
            return true;
        }

        public bool LocateMessageResponse(LocateResponse locateResponse)
        {
            //not provided by Tradier
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send Limit Order To Tradier
        /// </summary>
        /// <param name="limitOrder"></param>
        public void SendLimitOrder(LimitOrder limitOrder)
        {
            try
            {
                string orderTiff = OrderTif.DAY;
                if (!string.IsNullOrEmpty(limitOrder.OrderTif))
                {
                    orderTiff = limitOrder.OrderTif;
                }

                var orderId = _tradierManager.SendLimitOrder(limitOrder.OrderSide, limitOrder.OrderSize,
                    limitOrder.Security.Symbol,
                    limitOrder.LimitPrice, orderTiff);
                limitOrder.BrokerOrderID = orderId;
                _ordersMap.TryAdd(limitOrder.OrderID, limitOrder);
            }
            catch (Exception exception)
            {
                _logger.Error(exception,_type.FullName,"SendLimitOrder");
            }
        }

        /// <summary>
        /// Cancel Limit Order
        /// </summary>
        /// <param name="order"></param>
        public void CancelLimitOrder(Order order)
        {
            try
            {
                Order orderToCancel;
                if (_ordersMap.TryGetValue(order.OrderID, out orderToCancel))
                {
                    var status=_tradierManager.CancelOrder(orderToCancel.BrokerOrderID);
                    if (status == HttpStatusCode.OK)
                    {
                        if (CancellationArrived != null)
                        {
                            CancellationArrived((Order) orderToCancel.Clone());
                        }
                    }
                    else
                    {
                        if (OrderRejectionArrived != null)
                        {
                            OrderRejectionArrived(new Rejection(orderToCancel.Security, _provider) { OrderId = orderToCancel.OrderID });
                        }
                    }
                    //remove order
                    _ordersMap.TryRemove(order.OrderID, out orderToCancel);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception,_type.FullName,"CancelLimitOrder");
            }
        }
        #endregion
    }
}
