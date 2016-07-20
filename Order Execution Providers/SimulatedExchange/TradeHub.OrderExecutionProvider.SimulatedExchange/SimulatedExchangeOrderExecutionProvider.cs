using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EasyNetQ;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.OrderExecutionProvider;
using TradeHub.OrderExecutionProvider.SimulatedExchange.Service;
using TradeHub.SimulatedExchange.Common;
using TradeHub.SimulatedExchange.DomainObjects.Constant;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.OrderExecutionProvider.SimulatedExchange
{
    public class SimulatedExchangeOrderExecutionProvider : IMarketOrderProvider,ILimitOrderProvider
    {
        private Type _type = typeof(SimulatedExchangeOrderExecutionProvider);

        private CommunicationController _communicationController;
        private bool _isConnected;

        /// <summary>
        /// Keeps tracks of all the cancel orders
        /// Key = Order ID
        /// Value = TradeHub Orders
        /// </summary>
        private ConcurrentDictionary<string, Order> _cancelOrdersMap;

        public SimulatedExchangeOrderExecutionProvider()
        {
            // Initialize
            _cancelOrdersMap = new ConcurrentDictionary<string, Order>();
            _communicationController = new CommunicationController();

            //_communicationController.Connect();
            SubscribeRequiredEvents();
        }

        /// <summary>
        /// Starts the Connection to Simulated Exchange Order Execution Provider.
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            try
            {
                if(!_isConnected)
                    _communicationController.Connect();

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Starting Simulated Order Exchange Connector", _type.FullName, "Start");
                }
                //_communicationController.PublishOrderAdminMessage("OrderLogin");

                LoginArrivedFromSimulatedExchange();
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Start");
                return false;
            }
        }

        /// <summary>
        /// Hook all the required Queues of Simulated Exchange.
        /// </summary>
        private void SubscribeRequiredEvents()
        {
            try
            {
                _communicationController.OrderExecutionLoginRequest += LoginArrivedFromSimulatedExchange;

                _communicationController.ExecutionOrderReceived += ExecutionReceived;
                _communicationController.NewOrderStatusReceived += NewOrderArrived;
                _communicationController.RejectionOrderReceived += NewRejectionArrived;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeRequiredEvents");
            }
        }

        /// <summary>
        /// Receives Login Message from Simulated Exchange
        /// </summary>
        private void LoginArrivedFromSimulatedExchange()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Loggin Message Arrived From Simulated Exchange", _type.FullName,
                                "LoginArrivedFromSimulatedExchange");
                }
                if (LogonArrived != null)
                {
                    LogonArrived.Invoke(Common.Core.Constants.OrderExecutionProvider.SimulatedExchange);
                }
                _isConnected = true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "LoginArrivedFromSimulatedExchange");
            }
        }

        /// <summary>
        /// Terminate Connection from Simulated Exchange
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            try
            {
                // Publish Logout Message to Simulated Exchange
                _communicationController.PublishOrderAdminMessage("OrderLogout");

                _isConnected = false;

                if (LogoutArrived != null)
                {
                    LogoutArrived.Invoke(Common.Core.Constants.OrderExecutionProvider.SimulatedExchange);
                }

                // Clear cancel orders map
                _cancelOrdersMap.Clear();

                // Disconncet Communnication Controller
                _communicationController.Disconnect();

                return _isConnected;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Stop");
                return false;
            }
        }

        /// <summary>
        /// Returns the connection status
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            return _isConnected;
        }

        /// <summary>
        /// Sends Locate message Accepted/Rejected response to Broker
        /// </summary>
        /// <param name="locateResponse"> </param>
        /// <returns></returns>
        public bool LocateMessageResponse(LocateResponse locateResponse)
        {
            try
            {
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "LocateMessageResponse");
                return false;
            }
        }

        public event Action<string> LogonArrived;
        public event Action<string> LogoutArrived;
        public event Action<Rejection> OrderRejectionArrived;
        public event Action<Order> NewArrived;
        public event Action<Execution> ExecutionArrived;
        public event Action<Order> CancellationArrived;
        public event Action<Rejection> RejectionArrived;
        public event Action<LimitOrder> OnLocateMessage;
        public event Action<Position> OnPositionMessage;

        /// <summary>
        /// Sends the Limit order.
        /// </summary>
        /// <param name="limitOrder"></param>
        public void SendLimitOrder(LimitOrder limitOrder)
        {
            try
            {
                _communicationController.PublishLimitOrder(limitOrder);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendLimitOrder");
            }
        }

        /// <summary>
        /// Sends Cancel Order request
        /// </summary>
        /// <param name="order"></param>
        public void CancelLimitOrder(Order order)
        {
            try
            {
                _cancelOrdersMap.TryAdd(order.OrderID, order);

                // Change Order Status for cancelled order
                order.OrderStatus = TradeHubConstants.OrderStatus.CANCELLED;

                if (CancellationArrived != null)
                {
                    CancellationArrived(order);
                }

                //// Publish Cancel Order Request to Simulated Exchange
                //_communicationController.PublishCancelOrderRequest(order);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "CancelLimitOrder");
            }
        }

        /// <summary>
        /// Sends the market data.
        /// </summary>
        /// <param name="marketOrder"></param>
        public void SendMarketOrder(MarketOrder marketOrder)
        {
            try
            {
               _communicationController.PublishMarketOrder(marketOrder);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendMarketOrder");
            }
        }

        /// <summary>
        /// Rejection Arrived in OEE
        /// </summary>
        /// <param name="obj"></param>
        private void NewRejectionArrived(Rejection obj)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(obj.ToString(), _type.FullName, "NewRejectionArrived");
                }
                if (OrderRejectionArrived != null)
                {
                    OrderRejectionArrived.Invoke(obj);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "NewRejectionArrived");
            }
        }

        /// <summary>
        /// New Arrived
        /// </summary>
        /// <param name="obj"></param>
        private void NewOrderArrived(Order obj)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(obj.ToString(), _type.FullName, "NewOrderArrived");
                }
                if (NewArrived != null)
                {
                    NewArrived.Invoke(obj);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "NewOrderArrived");
            }
        }

        /// <summary>
        /// New Execution Arrived
        /// </summary>
        /// <param name="execution"></param>
        public void ExecutionReceived(Execution execution)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(execution.ToString(), _type.FullName, "ExecutionReceived");
                }

                // Check if the order is already cancelled
                if (_cancelOrdersMap.ContainsKey(execution.Order.OrderID))
                {
                    Order order;
                    _cancelOrdersMap.TryRemove(execution.Order.OrderID, out order);
                    return;
                }

                // Rasie Execution Event
                if (ExecutionArrived != null)
                {
                    ExecutionArrived.Invoke(execution);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ExecutionReceived");
            }
        }

    }
}
