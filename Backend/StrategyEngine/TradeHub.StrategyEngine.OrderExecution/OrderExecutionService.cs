/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* TradeSharp is a C# based data feed and broker neutral Algorithmic 
* Trading Platform that lets trading firms or individuals automate 
* any rules based trading strategies in stocks, forex and ETFs. 
* TradeSharp allows users to connect to providers like Tradier Brokerage, 
* IQFeed, FXCM, Blackwood, Forexware, Integral, HotSpot, Currenex, 
* Interactive Brokers and more. 
* Key features: Place and Manage Orders, Risk Management, 
* Generate Customized Reports etc 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories;
using TradeHub.Common.Core.Utility;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.OrderExecutionEngine.Client.Service;
using TradeHub.StrategyEngine.OrderExecution.Utility;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.StrategyEngine.OrderExecution
{
    /// <summary>
    /// Responsible for handling all the Order requests
    /// </summary>
    public class OrderExecutionService : IEventHandler<RabbitMqRequestMessage>, IDisposable
    {
        private Type _type = typeof(OrderExecutionService);
        private AsyncClassLogger _asyncClassLogger;
        
        #region Events

        // ReSharper disable InconsistentNaming
        private event Action _connected;
        private event Action _disconnected;
        private event Action<string> _logonArrived;
        private event Action<string> _logoutArrived;
        private event Action<Order> _newArrived;
        private event Action<Order> _cancellationArrived;
        private event Action<Execution> _executionArrived;
        private event Action<Rejection> _rejectionArrived; 
        private event Action<LimitOrder> _locateMessageArrived; 
        private event Action<byte[]> _sendOrderRequests; 
        private event Action<MarketOrder> _sendMarketOrderRequest; 
        private event Action<LimitOrder> _sendLimitOrderRequest; 
        private event Action<Order> _sendCancelOrderRequest; 
        // ReSharper restore InconsistentNaming

        public event Action Connected
        {
            add
            {
                if (_connected == null)
                {
                    _connected += value;
                }
            }
            remove { _connected -= value; }
        }

        public event Action Disconnected
        {
            add
            {
                if (_disconnected == null)
                {
                    _disconnected += value;
                }
            }
            remove { _disconnected -= value; }
        }

        public event Action<string> LogonArrived
        {
            add
            {
                if (_logonArrived == null)
                {
                    _logonArrived += value;
                }
            }
            remove { _logonArrived -= value; }
        }

        public event Action<string> LogoutArrived
        {
            add
            {
                if (_logoutArrived == null)
                {
                    _logoutArrived += value;
                }
            }
            remove { _logoutArrived -= value; }
        }

        public event Action<Order> NewArrived
        {
            add
            {
                if (_newArrived == null)
                {
                    _newArrived += value;
                }
            }
            remove { _newArrived -= value; }
        }

        public event Action<Order> CancellationArrived
        {
            add
            {
                if (_cancellationArrived == null)
                {
                    _cancellationArrived += value;
                }
            }
            remove { _cancellationArrived -= value; }
        }

        public event Action<Execution> ExecutionArrived
        {
            add
            {
                if (_executionArrived == null)
                {
                    _executionArrived += value;
                }
            }
            remove { _executionArrived -= value; }
        }

        public event Action<Rejection> RejectionArrived
        {
            add
            {
                if (_rejectionArrived== null)
                {
                    _rejectionArrived += value;
                }
            }
            remove { _rejectionArrived -= value; }
        }

        public event Action<LimitOrder> LocateMessageArrived
        {
            add
            {
                if (_locateMessageArrived == null)
                {
                    _locateMessageArrived += value;
                }
            }
            remove { _locateMessageArrived -= value; }
        }

        #endregion

        #region Disruptor

        /// <summary>
        /// Provides communication with Order Execution Engine
        /// </summary>
        private OrderExecutionEngineClient _executionEngineClient;

        // Disruptor Ring Buffer Size 
        private readonly int _ringSize = 65536; // Must be multiple of 2

        // Handles order request message
        private Disruptor<RabbitMqRequestMessage> _orderDisruptor;
        // Ring buffer to be used with disruptor
        private RingBuffer<RabbitMqRequestMessage> _orderRingBuffer;

        #endregion

        /// <summary>
        /// Indicates whether the order execution service is connected to OEE-Server or not
        /// </summary>
        private bool _isConnected = false;

        private bool _disposed = false;

        /// <summary>
        /// Keeps tracks of all the client orders
        /// Key = Order ID
        /// Value = TradeHub Orders
        /// </summary>
        private ConcurrentDictionary<string, Order> _ordersMap;

        /// <summary>
        /// Indicates whether the order execution service is connected to OEE-Server or not
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; }
        }

        /// <summary>
        /// Keeps tracks of all the client orders
        /// Key = Order ID
        /// Value = TradeHub Orders
        /// </summary>
        public ReadOnlyConcurrentDictionary<string, Order> OrdersMap
        {
            get { return new ReadOnlyConcurrentDictionary<string, Order>(_ordersMap); }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public OrderExecutionService()
        {
            _asyncClassLogger = new AsyncClassLogger("OrderExecutionService");
            // Set logging level
            _asyncClassLogger.SetLoggingLevel();
            // Set logging path
            _asyncClassLogger.LogDirectory(TradeHubConstants.DirectoryStructure.CLIENT_LOGS_LOCATION);

            InitializeDistuptor(new IEventHandler<RabbitMqRequestMessage>[] { this });

            // Initialize local map
            _ordersMap = new ConcurrentDictionary<string, Order>();
            
            // Register Local Event
            _sendMarketOrderRequest += SendMarketOrderRequest;
            _sendLimitOrderRequest += SendLimitOrderRequest;
            _sendCancelOrderRequest += SendCancelOrderRequest;
            _sendOrderRequests += SendOrderRequests;
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="orderExecutionEngineClient">OEE-Client for communication with the OEE-Server</param>
        public OrderExecutionService(OrderExecutionEngineClient orderExecutionEngineClient)
        {
            _asyncClassLogger = new AsyncClassLogger("OrderExecutionService");
            // Set logging level
            _asyncClassLogger.SetLoggingLevel();
            // Set logging path
            _asyncClassLogger.LogDirectory(TradeHubConstants.DirectoryStructure.CLIENT_LOGS_LOCATION);

            InitializeDistuptor(new IEventHandler<RabbitMqRequestMessage>[] {this});

            // save order execution client 
            _executionEngineClient = orderExecutionEngineClient;

            // Initialize local map
            _ordersMap = new ConcurrentDictionary<string, Order>();

            // Register Local Event
            _sendMarketOrderRequest += SendMarketOrderRequest;
            _sendLimitOrderRequest += SendLimitOrderRequest;
            _sendCancelOrderRequest += SendCancelOrderRequest;
            _sendOrderRequests += SendOrderRequests;

            // Register required OEE-Client Events
            RegisterExecutionEngineClientEvents();
        }

        /// <summary>
        /// Initialize Disruptor and adds required Handler
        /// </summary>
        public void InitializeDistuptor(IEventHandler<RabbitMqRequestMessage>[] handler)
        {
            if (_orderDisruptor != null)
                _orderDisruptor.Shutdown();
            
            // Initialize Disruptor
            _orderDisruptor = new Disruptor<RabbitMqRequestMessage>(() => new RabbitMqRequestMessage(), _ringSize, TaskScheduler.Default);

            // Add Disruptor Consumer
            _orderDisruptor.HandleEventsWith(handler);

            // Start Disruptor
            _orderRingBuffer = _orderDisruptor.Start();
        }

        #region Override requests

        /// <summary>
        /// Overrides the send order request calls
        /// </summary>
        public void OverrideOrderRequest(Action<byte[]> sendOrderAction)
        {
            // Remove old event
            _sendOrderRequests -= SendOrderRequests;

            // Add new event
            _sendOrderRequests += sendOrderAction;
        }

        /// <summary>
        /// Overrides Market Order request calls
        /// </summary>
        /// <param name="marketOrderAction"></param>
        public void OverrideMarketOrderRequest(Action<MarketOrder> marketOrderAction)
        {
            // Remove old event
            _sendMarketOrderRequest -= SendMarketOrderRequest;

            // Add new event
            _sendMarketOrderRequest += marketOrderAction;
        }

        /// <summary>
        /// Overrides Limit Order request calls
        /// </summary>
        /// <param name="limitOrderAction"></param>
        public void OverrideLimitOrderRequest(Action<LimitOrder> limitOrderAction)
        {
            // Remove old event
            _sendLimitOrderRequest -= SendLimitOrderRequest;

            // Add new event
            _sendLimitOrderRequest += limitOrderAction;
        }

        /// <summary>
        /// Overrides Cancel Order request calls
        /// </summary>
        /// <param name="cancelOrderAction"></param>
        public void OverrideCancelOrderRequest(Action<Order> cancelOrderAction)
        {
            // Remove old event
            _sendCancelOrderRequest -= SendCancelOrderRequest;

            // Add new event
            _sendCancelOrderRequest += cancelOrderAction;
        }

        #endregion

        /// <summary>
        /// Initializes necessary components for communication - Needs to be called if the Service was stopped
        /// </summary>
        public void InitializeService()
        {
            // Initialize Distruptor
            InitializeDistuptor(new IEventHandler<RabbitMqRequestMessage>[] { this });

            // Initialize Client
            _executionEngineClient.Initialize();
        }

        /// <summary>
        /// Starts Order Execution Service
        /// </summary>
        public bool StartService()
        {
            try
            {
                if (_isConnected)
                {
                    if (_asyncClassLogger.IsInfoEnabled)
                    {
                        _asyncClassLogger.Info("Order execution service already running.", _type.FullName, "StartService");
                    }

                    return true;
                }

                // Start OEE-Client
                _executionEngineClient.Start();

                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Order Execution service started.", _type.FullName, "StartService");
                }

                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "StartService");
                return false;
            }
        }

        /// <summary>
        /// Stops Order Execution Service
        /// </summary>
        public bool StopService()
        {
            try
            {
                if (_executionEngineClient != null)
                {
                    // Stop OEE-Client
                    _executionEngineClient.Shutdown();
                }

                // Clear local map
                _ordersMap.Clear();

                // Shutdown disruptor
                _orderDisruptor.Shutdown();

                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Order Execution service stopped.", _type.FullName, "StopService");
                }

                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "StopService");
                return false;
            }
        }

        /// <summary>
        /// Registers required OEE-Client events
        /// </summary>
        private void RegisterExecutionEngineClientEvents()
        {
            _executionEngineClient.ServerConnected += OnServerConnected;
            _executionEngineClient.ServerDisconnected += OnServerDisconnected;
            _executionEngineClient.LogonArrived += OnLogonArrived;
            _executionEngineClient.LogoutArrived += OnLogoutArrived;
            _executionEngineClient.NewArrived += OnNewArrived;
            _executionEngineClient.CancellationArrived += OnCancellationArrived;
            _executionEngineClient.ExecutionArrived += OnExecutionArrived;
            _executionEngineClient.RejectionArrived += OnRejectionArrived;
            _executionEngineClient.LocateMessageArrived += OnLocateMessageArrived;
        }

        /// <summary>
        /// Unregisters required OEE-Client events
        /// </summary>
        private void UnregisterExecutionEngineClientEvents()
        {
            _executionEngineClient.ServerConnected -= OnServerConnected;
            _executionEngineClient.ServerDisconnected -= OnServerDisconnected;
            _executionEngineClient.LogonArrived -= OnLogonArrived;
            _executionEngineClient.LogoutArrived -= OnLogoutArrived;
        }

        #region OEE-Client Events

        /// <summary>
        /// Called when OEE-Client successfully connects with OEE-Server
        /// </summary>
        private void OnServerConnected()
        {
            try
            {
                _isConnected = true;

                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Successfully connected with OEE-Server.", _type.FullName, "OnServerConnected");
                }

                if (_connected != null)
                {
                    _connected();
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnServerConnected");
            }
        }

        /// <summary>
        /// Called when OEE-Client disconnects with OEE-Server
        /// </summary>
        private void OnServerDisconnected()
        {
            try
            {
                _isConnected = false;

                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Disconnected with OEE-Server", _type.FullName, "OnServerDisconnected");
                }

                if (_disconnected != null)
                {
                    _disconnected();
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnServerDisconnected");
            }
        }

        /// <summary>
        /// Called when OEE-Client receives logon message from OEE-Server
        /// </summary>
        private void OnLogonArrived(string providerName)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Logon message arrived from: " + providerName, _type.FullName, "OnLogonArrived");
                }

                if (_logonArrived != null)
                {
                    _logonArrived(providerName);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnLogonArrived");
            }
        }

        /// <summary>
        /// Called when OEE-Client receives logout message from OEE-Server
        /// </summary>
        private void OnLogoutArrived(string providerName)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Logout message arrived from: " + providerName, _type.FullName, "OnLogoutArrived");
                }

                if (_logoutArrived != null)
                {
                    _logoutArrived(providerName);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnLogoutArrived");
            }
        }

        /// <summary>
        /// Called when OEE-Client receives new order event from OEE-Server
        /// </summary>
        /// <param name="order">TradeHub Order containing accepted/submitted order info</param>
        private void OnNewArrived(Order order)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New Order Event received: " + order, _type.FullName, "OnNewArrived");
                }

                Order clientOrder;
                if (_ordersMap.TryGetValue(order.OrderID, out clientOrder))
                {
                    // Change Order Status for accepted order
                    order.OrderStatus = TradeHubConstants.OrderStatus.SUBMITTED;

                    if (_newArrived != null)
                    {
                        _newArrived(order);
                    }

                    return;
                }
                
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("No client order available for the arrived order id: " + order.OrderID, _type.FullName, "OnNewArrived");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnNewArrived");
            }
        }

        /// <summary>
        /// Called when OEE-Client receives order cancellation event from OEE-Server
        /// </summary>
        /// <param name="order">TradeHub Order containing cancelled order info</param>
        private void OnCancellationArrived(Order order)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Order cancellation event received: " + order, _type.FullName, "OnCancellationArrived");
                }

                Order clientOrder;
                if (_ordersMap.TryRemove(order.OrderID, out clientOrder))
                {
                    // Change Order Status for cancelled order
                    order.OrderStatus = TradeHubConstants.OrderStatus.CANCELLED;

                    if (_cancellationArrived != null)
                    {
                        _cancellationArrived(order);
                    }

                    return;
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("No client order available for the arrived order id: " + order.OrderID, _type.FullName, "OnCancellationArrived");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnCancellationArrived");
            }
        }

        /// <summary>
        /// Called when OEE-Client receives order execution event from OEE-Server
        /// </summary>
        /// <param name="execution">TradeHub Execution containg order execution details</param>
        private void OnExecutionArrived(Execution execution)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Order execution event received: " + execution.Order.OrderID, _type.FullName, "OnExecutionArrived");
                }

                // Change Order Status for partial executed order
                execution.Order.OrderStatus = TradeHubConstants.OrderStatus.PARTIALLY_EXECUTED;

                Order clientOrder;
                if (_ordersMap.TryGetValue(execution.Order.OrderID, out clientOrder))
                {
                    // Remove from internal map if full fill is received
                    if (execution.Fill.LeavesQuantity.Equals(0))
                    {
                        _ordersMap.TryRemove(execution.Order.OrderID, out clientOrder);

                        // Change Order Status for complete executed order
                        execution.Order.OrderStatus = TradeHubConstants.OrderStatus.EXECUTED;
                    }
                }

                //// Remove from internal map if full fill is received
                //if (execution.Fill.LeavesQuantity.Equals(0))
                //{
                //    // Change Order Status for complete executed order
                //    execution.Order.OrderStatus = TradeHubConstants.OrderStatus.EXECUTED;
                //}
                
                if (_executionArrived != null)
                {
                    _executionArrived(execution);
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Order execution event fired: " + execution.Order.OrderID, _type.FullName, "OnExecutionArrived");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnExecutionArrived");
            }
        }

        /// <summary>
        /// Called when OEE-Client receives rejection event from OEE-Server
        /// </summary>
        /// <param name="rejection">TradeHub Rejection containing rejection message details</param>
        private void OnRejectionArrived(Rejection rejection)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Rejection event received: " + rejection, _type.FullName, "OnRejectionArrived");
                }

                Order clientOrder;
                _ordersMap.TryRemove(rejection.OrderId, out clientOrder);
             
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("No client order available for the arrived order id: " + rejection.OrderId,
                                 _type.FullName, "OnRejectionArrived");
                }

                if (_rejectionArrived != null)
                {
                    _rejectionArrived(rejection);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnRejectionArrived");
            }
        }

        /// <summary>
        /// Called when OEE-Client receives Locate Message from OEE-Server
        /// </summary>
        /// <param name="locateMessage">TradeHub LimitOrder containing Locate Message info</param>
        private void OnLocateMessageArrived(LimitOrder locateMessage)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Locate message event received: " + locateMessage, _type.FullName, "OnLocateMessageArrived");
                }

                // Raise Locate Message event to notify listeners
                if (_locateMessageArrived != null)
                {
                    _locateMessageArrived(locateMessage);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnLocateMessageArrived");
            }
        }

        #endregion

        #region Incoming requests for OEE-Server

        /// <summary>
        /// Sends Login request to OEE
        /// </summary>
        /// <param name="login">TradeHub Login object</param>
        public bool Login(Login login)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Sending Login request for: " + login.OrderExecutionProvider, _type.FullName, "Login");
                }

                // Check if OEE-Client is connected to OEE
                if (_isConnected)
                {
                    // Send login request to OEE
                    _executionEngineClient.SendLoginRequest(login);
                    return true;
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Request not sent to OEE as OEE-Client is not connected.", _type.FullName, "Login");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Login");
                return false;
            }
        }

        /// <summary>
        /// Sends Logout request to OEE
        /// </summary>
        /// <param name="logout">TradeHub Logout object</param>
        public bool Logout(Logout logout)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Sending logout request for: " + logout.OrderExecutionProvider, _type.FullName, "Logout");
                }

                // Check if OEE-Client is connected to OEE
                if (_isConnected)
                {
                    // Send logout request to OEE
                    _executionEngineClient.SendLogoutRequest(logout);
                    return true;
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Request not sent to OEE as OEE-Client is not connected.", _type.FullName, "Logout");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Logout");
                return false;
            }
        }

        /// <summary>
        /// Sends Market Order to OEE
        /// </summary>
        /// <param name="marketOrder">TradeHub Market Order</param>
        public bool SendOrder(MarketOrder marketOrder)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New market order request received: " + marketOrder, _type.FullName, "SendOrder");
                }

                // Check if OEE-Client is connected to OEE
                if (_isConnected || marketOrder.OrderExecutionProvider.Equals(TradeHubConstants.OrderExecutionProvider.SimulatedExchange))
                {
                    // Update internal orders map
                    _ordersMap.TryAdd(marketOrder.OrderID, marketOrder);
                    
                    if (_asyncClassLogger.IsDebugEnabled)
                    {
                        _asyncClassLogger.Debug("Total active orders: " + _ordersMap.Count, _type.FullName, "SendOrder");
                    }

                    //// Send Market Order to OEE-Server
                    //_executionEngineClient.SendMarketOrderRequest(marketOrder);
                    
                    //Send Market Order to be forwarded to Client
                    _sendMarketOrderRequest(marketOrder);

                    return true;
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Request not sent to OEE as OEE-Client is not connected.", _type.FullName, "SendOrder");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendOrder");
                return false;
            }
        }

        /// <summary>
        /// Sends Limit Order to OEE
        /// </summary>
        /// <param name="limitOrder">TradeHub Limit Order</param>
        public bool SendOrder(LimitOrder limitOrder)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New limit order request received: " + limitOrder, _type.FullName, "SendOrder");
                }

                // Check if OEE-Client is connected to OEE
                if (_isConnected || limitOrder.OrderExecutionProvider.Equals(TradeHubConstants.OrderExecutionProvider.SimulatedExchange))
                {
                    // Update internal orders map
                    _ordersMap.TryAdd(limitOrder.OrderID, limitOrder);

                    if (_asyncClassLogger.IsDebugEnabled)
                    {
                        _asyncClassLogger.Debug("Total active orders: " + _ordersMap.Count, _type.FullName, "SendOrder");
                    }

                    //// Send Limit Order to OEE-Server
                    //_executionEngineClient.SendLimitOrderRequest(limitOrder);

                    //Send Limit Order to be forwarded to Client
                    _sendLimitOrderRequest(limitOrder);

                    return true;
                }

                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Request not sent to OEE as OEE-Client is not connected.", _type.FullName, "SendOrder");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendOrder");
                return false;
            }
        }

        /// <summary>
        /// Sends Cancel Order request to OEE
        /// </summary>
        /// <param name="orderId">ID of the order which is to be cancelled</param>
        public bool CancelOrder(string orderId)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New cancel order request received: " + orderId, _type.FullName, "CancelOrder");
                }

                    Order cancelOrder;
                    // Get order to be cancelled
                    if (_ordersMap.TryGetValue(orderId, out cancelOrder))
                    {
                        // Check if OEE-Client is connected to OEE
                        if (_isConnected || cancelOrder.OrderExecutionProvider.Equals(
                                TradeHubConstants.OrderExecutionProvider.SimulatedExchange))
                        {
                            //Send Cancel Order to be forwarded to Client
                            _sendCancelOrderRequest(cancelOrder);

                            return true;
                        }

                        if (_asyncClassLogger.IsInfoEnabled)
                        {
                            _asyncClassLogger.Info("No active order found for the given ID: " + orderId, _type.FullName,
                                                   "CancelOrder");
                        }
                    }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Request not sent to OEE as OEE-Client is not connected.", _type.FullName, "CancelOrder");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "CancelOrder");
                return false;
            }
        }

        /// <summary>
        /// Sends Locate Response to OEE
        /// </summary>
        /// <param name="locateResponse">TradeHub LocateResponse for the Locate Message</param>
        public bool SendLocateResponse(LocateResponse locateResponse)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New locate response received: " + locateResponse, _type.FullName, "SendLocateResponse");
                }

                // Check if OEE-Client is connected to OEE
                if (_isConnected)
                {
                    // Send Locate Response to OEE-Server
                    _executionEngineClient.SendLocateResponse(locateResponse);
                    return true;
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Request not sent to OEE as OEE-Client is not connected.", _type.FullName, "SendLocateResponse");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendLocateResponse");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Converts incoming Market order to Byte stream
        /// Sends to the singular order request sender function
        /// </summary>
        /// <param name="marketOrder">TardeHub MarketOrder</param>
        private void SendMarketOrderRequest(MarketOrder marketOrder)
        {
            // Convert MarketOrder to Byte Stream and pass it onto distuptor
            byte[] responseBytes = Encoding.UTF8.GetBytes(marketOrder.DataToPublish());
            //SendOrderRequests(responseBytes);
            _sendOrderRequests(responseBytes);
        }

        /// <summary>
        /// Converts incoming Limit order to Byte stream
        /// Sends to the singular order request sender function
        /// </summary>
        /// <param name="limitOrder">TradeHub LimitOrder</param>
        private void SendLimitOrderRequest(LimitOrder limitOrder)
        {
            // Convert MarketOrder to Byte Stream and pass it onto distuptor
            byte[] responseBytes = Encoding.UTF8.GetBytes(limitOrder.DataToPublish());
            //SendOrderRequests(responseBytes);
            _sendOrderRequests(responseBytes);
        }

        /// <summary>
        /// Converts incoming Cancel Order to Byte stream
        /// Sends to the singular order request sender function
        /// </summary>
        /// <param name="order">TradeHub Order</param>
        private void SendCancelOrderRequest(Order order)
        {
            // Convert MarketOrder to Byte Stream and pass it onto distuptor
            byte[] responseBytes = Encoding.UTF8.GetBytes(order.DataToPublish("Cancel"));
            //SendOrderRequests(responseBytes);
            _sendOrderRequests(responseBytes);
        }

        /// <summary>
        /// Sends Order Requests to OEE using OEE-Client
        /// </summary>
        private void SendOrderRequests(byte[] messageBytes)
        {
            // Get next sequence number
            long sequenceNo = _orderRingBuffer.Next();

            // Get object from ring buffer
            RabbitMqRequestMessage entry = _orderRingBuffer[sequenceNo];

            // Initialize property
            entry.Message= new byte[messageBytes.Length];

            // Update object values
            messageBytes.CopyTo(entry.Message,0);

            // Publish sequence number for which the object is updated
            _orderRingBuffer.Publish(sequenceNo);
        }

        /// <summary>
        /// Clears the order map
        /// </summary>
        public void ClearOrderMap()
        {
            _ordersMap.Clear();
        }

        /// <summary>
        /// Generates a Unique Order ID across the system
        /// </summary>
        /// <returns>Order ID</returns>
        public string GetOrderId()
        {
            // Request Order ID Generator for next Unique ID
            return OrderIdGenerator.GetId((_executionEngineClient == null) ? string.Empty : _executionEngineClient.AppId);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    StopService();
                }
                // Release unmanaged resources.
                _disposed = true;
            }
        }

        #region Implementation of IEventHandler<in RabbitMqMessage>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(RabbitMqRequestMessage data, long sequence, bool endOfBatch)
        {
            // Forward order requests to Execution Client 
            _executionEngineClient.SendOrderRequests(data);
        }

        #endregion
    }
}
