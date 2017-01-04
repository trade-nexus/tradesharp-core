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


using System;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.FactoryMethods;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.StrategyEngine.OrderExecution;
using TradeSharp.UI.Common.Models;

namespace TradeSharp.ServiceControllers.Managers
{
    /// <summary>
    /// Provides Communication access with Order Execution Server
    /// </summary>
    internal class OrderExecutionManager : IDisposable
    {
        /// <summary>
        /// Provides communication access with Order Execution Server
        /// </summary>
        private readonly OrderExecutionService _orderExecutionService;

        private bool _disposed = false;

        #region Events

        // ReSharper disable InconsistentNaming
        private event Action _connectedEvent;
        private event Action _disconnectedEvent;
        private event Action<string> _logonArrivedEvent;
        private event Action<string> _logoutArrivedEvent;
        private event Action<Order> _orderAcceptedEvent;
        private event Action<Execution> _executionArrivedEvent;
        private event Action<Order> _cancellationArrivedEvent;
        private event Action<Rejection> _rejectionArrivedEvent; 
        // ReSharper restore InconsistentNaming

        public event Action ConnectedEvent
        {
            add
            {
                if (_connectedEvent == null)
                {
                    _connectedEvent += value;
                }
            }
            remove { _connectedEvent -= value; }
        }

        public event Action DisconnectedEvent
        {
            add
            {
                if (_disconnectedEvent == null)
                {
                    _disconnectedEvent += value;
                }
            }
            remove { _disconnectedEvent -= value; }
        }

        public event Action<string> LogonArrivedEvent
        {
            add
            {
                if (_logonArrivedEvent == null)
                {
                    _logonArrivedEvent += value;
                }
            }
            remove { _logonArrivedEvent -= value; }
        }

        public event Action<string> LogoutArrivedEvent
        {
            add
            {
                if (_logoutArrivedEvent == null)
                {
                    _logoutArrivedEvent += value;
                }
            }
            remove { _logoutArrivedEvent -= value; }
        }

        public event Action<Order> OrderAcceptedEvent
        {
            add
            {
                if (_orderAcceptedEvent == null)
                {
                    _orderAcceptedEvent += value;
                }
            }
            remove { _orderAcceptedEvent -= value; }
        }

        public event Action<Execution> ExecutionArrivedEvent
        {
            add
            {
                if (_executionArrivedEvent == null)
                {
                    _executionArrivedEvent += value;
                }
            }
            remove { _executionArrivedEvent -= value; }
        }

        public event Action<Order> CancellationArrivedEvent
        {
            add
            {
                if (_cancellationArrivedEvent == null)
                {
                    _cancellationArrivedEvent += value;
                }
            }
            remove { _cancellationArrivedEvent -= value; }
        }

        public event Action<Rejection> RejectionArrivedEvent
        {
            add
            {
                if (_rejectionArrivedEvent == null)
                {
                    _rejectionArrivedEvent += value;
                }
            }
            remove { _rejectionArrivedEvent -= value; }
        }

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="orderExecutionService">Provides communication access with Order Execution Server</param>
        public OrderExecutionManager(OrderExecutionService orderExecutionService)
        {
            // Save object reference
            _orderExecutionService = orderExecutionService;

            SubscribeExecutionServiceEvents();

            // Start Service
            _orderExecutionService.StartService();
        }

        /// <summary>
        /// Register Order Execution Service Events
        /// </summary>
        private void SubscribeExecutionServiceEvents()
        {
            // Makes sure that events are only hooked once
            UnsubscribeExecutionServiceEvents();

            _orderExecutionService.Connected += OnConnected;
            _orderExecutionService.Disconnected += OnDisconnected;

            _orderExecutionService.LogonArrived += OnLogonArrived;
            _orderExecutionService.LogoutArrived += OnLogoutArrived;

            _orderExecutionService.NewArrived += OnOrderAccepted;
            _orderExecutionService.ExecutionArrived += OnExecutionArrived;
            _orderExecutionService.RejectionArrived += OnRejectionArrived;
            _orderExecutionService.CancellationArrived += OnCancellationArrived;
        }

        /// <summary>
        /// Unsubscribe Order Execution Service Events
        /// </summary>
        private void UnsubscribeExecutionServiceEvents()
        {
            _orderExecutionService.Connected -= OnConnected;
            _orderExecutionService.Disconnected -= OnDisconnected;

            _orderExecutionService.LogonArrived -= OnLogonArrived;
            _orderExecutionService.LogoutArrived -= OnLogoutArrived;

            _orderExecutionService.NewArrived -= OnOrderAccepted;
            _orderExecutionService.ExecutionArrived -= OnExecutionArrived;
            _orderExecutionService.RejectionArrived -= OnRejectionArrived;
            _orderExecutionService.CancellationArrived -= OnCancellationArrived;
        }

        #region Connect/Disconnect methods

        /// <summary>
        /// Establishes connection with Order Execution Server
        /// </summary>
        public void Connect()
        {
            // Initialize service to re-establish connection
            _orderExecutionService.InitializeService();

            // Start Service
            _orderExecutionService.StartService();
        }

        /// <summary>
        /// Terminates connection with Order Execution Server
        /// </summary>
        public void Disconnect()
        {
            _orderExecutionService.StopService();
        }

        /// <summary>
        /// Sends Connection request to Order Execution Server
        /// </summary>
        /// <param name="providerName">Order Execution Provider to connect</param>
        public void Connect(string providerName)
        {
            // Create a new login message
            Login login = new Login()
            {
                OrderExecutionProvider = providerName
            };

            _orderExecutionService.Login(login);
        }

        /// <summary>
        /// Sends request to Order Execution Server for disconnecting given order execution provider
        /// </summary>
        /// <param name="providerName">Order Execution Provider to disconnect</param>
        public void Disconnect(string providerName)
        {
            // Create a new logout message
            Logout logout = new Logout()
            {
                OrderExecutionProvider = providerName
            };

            _orderExecutionService.Logout(logout);
        }

        #endregion

        #region Incoming Order Requests

        /// <summary>
        /// Sends a new Market Order Request to 'Order Execution Server'
        /// </summary>
        /// <param name="orderDetails">Contains market order information</param>
        public void MarketOrderRequests(OrderDetails orderDetails)
        {
            // Get new Order ID
            orderDetails.ID = _orderExecutionService.GetOrderId();

            // Create Market Order object to be sent to 'Order Execution Service'
            MarketOrder marketOrder = OrderMessage.GenerateMarketOrder(orderDetails.ID, orderDetails.Security, orderDetails.Side,
                orderDetails.Quantity, orderDetails.Provider);

            // Send Request to Server
            _orderExecutionService.SendOrder(marketOrder);
        }

        /// <summary>
        /// Sends a new Limit Order Request to 'Order Execution Server'
        /// </summary>
        /// <param name="orderDetails">Contains limit order information</param>
        public void LimitOrderRequest(OrderDetails orderDetails)
        {
            // Get new Order ID
            orderDetails.ID = _orderExecutionService.GetOrderId();

            // Create Limit Order object to be sent to 'Order Execution Service'
            LimitOrder limitOrder = OrderMessage.GenerateLimitOrder(orderDetails.ID, orderDetails.Security, orderDetails.Side,
                orderDetails.Quantity, orderDetails.Price, orderDetails.Provider);

            limitOrder.OrderTif = OrderTif.GTC;

            // Send Reques to Server
            _orderExecutionService.SendOrder(limitOrder);
        }

        /// <summary>
        /// Sends order cancellation request to 'Order Execution Server'
        /// </summary>
        /// <param name="orderId">ID for the order to be cancelled</param>
        public void CancelOrderRequest(string orderId)
        {
            _orderExecutionService.CancelOrder(orderId);
        }

        #endregion

        #region Order Execution Service Events

        /// <summary>
        /// Called when client is connected to Server
        /// </summary>
        private void OnConnected()
        {
            if (_connectedEvent != null)
            {
                _connectedEvent();
            }
        }

        /// <summary>
        /// Called when client is disconnected from Server
        /// </summary>
        private void OnDisconnected()
        {
            if (_disconnectedEvent != null)
            {
                _disconnectedEvent();
            }
        }

        /// <summary>
        /// Called when requested order execution provider is successfully 'Logged IN'
        /// </summary>
        /// <param name="providerName">Order Execution Provider name</param>
        private void OnLogonArrived(string providerName)
        {
            if (_logonArrivedEvent != null)
            {
                _logonArrivedEvent(providerName);
            }
        }

        /// <summary>
        /// Called when requested order execution provider is successfully 'Logged OUT'
        /// </summary>
        /// <param name="providerName">Order Execution Provider name</param>
        private void OnLogoutArrived(string providerName)
        {
            if (_logoutArrivedEvent != null)
            {
                _logoutArrivedEvent(providerName);
            }
        }

        /// <summary>
        /// Called when the requested order is accepted by respective Order Execution Provider
        /// </summary>
        /// <param name="order">Contains accepted order details</param>
        private void OnOrderAccepted(Order order)
        {
            if (_orderAcceptedEvent != null)
            {
                _orderAcceptedEvent(order);
            }
        }

        /// <summary>
        /// Called when order execution is receievd from 'Order Execution Server'
        /// </summary>
        /// <param name="execution">Contains execution details</param>
        private void OnExecutionArrived(Execution execution)
        {
            if (_executionArrivedEvent != null)
            {
                _executionArrivedEvent(execution);
            }
        }

        /// <summary>
        /// Called when requested order is rejected
        /// </summary>
        /// <param name="rejection">Contains rejection details</param>
        private void OnRejectionArrived(Rejection rejection)
        {
            if (_rejectionArrivedEvent != null)
            {
                _rejectionArrivedEvent(rejection);
            }
        }

        /// <summary>
        /// Called when order cancellaiton request is successful
        /// </summary>
        /// <param name="order">Contains cancelled order details</param>
        private void OnCancellationArrived(Order order)
        {
            if (_cancellationArrivedEvent != null)
            {
                _cancellationArrivedEvent(order);
            }
        }

        #endregion

        /// <summary>
        /// Stops all Order Execution activites and closes open connections
        /// </summary>
        public void Stop()
        {
            _orderExecutionService.StopService();
            _orderExecutionService.Dispose();
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
                    _orderExecutionService.StopService();
                }
                // Release unmanaged resources.
                _disposed = true;
            }
        }
    }
}
