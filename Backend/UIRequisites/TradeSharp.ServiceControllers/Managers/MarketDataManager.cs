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
using System.Collections.Generic;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.FactoryMethods;
using TradeHub.Common.Core.Utility;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.StrategyEngine.HistoricalData;
using TradeHub.StrategyEngine.MarketData;
using TradeSharp.UI.Common.ValueObjects;

namespace TradeSharp.ServiceControllers.Managers
{
    /// <summary>
    /// Provides functionality for Market Data related queries and response
    /// </summary>
    internal class MarketDataManager
    {
        /// <summary>
        /// Provides communication access with Market Data Server live data
        /// </summary>
        private readonly MarketDataService _marketDataService;

        /// <summary>
        /// Provides communication access with Market Data Server for historical data
        /// </summary>
        private readonly HistoricalDataService _historicalDataService;

        /// <summary>
        /// Responsible for creating ID's for market data requests
        /// </summary>
        private IMarketDataIdGenerator _idGenerator;

        /// <summary>
        /// Contians Bar Parameters for each bar request
        /// KEY: Bar Request ID
        /// VALUE: Bar Parameters <see cref="BarParameters"/>
        /// </summary>
        private Dictionary<string, BarParameters> _barParametersMap; 

        #region Events

        // ReSharper disable InconsistentNaming
        private event Action _connectedEvent;
        private event Action _disconnectedEvent;
        private event Action<string> _logonArrivedEvent;
        private event Action<string> _logoutArrivedEvent;
        private event Action<Tick> _tickArrivedEvent;
        private event Action<BarDetail> _barArrivedEvent;
        private event Action<HistoricBarData> _historicalDataArrivedEvent;
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

        public event Action<Tick> TickArrivedEvent
        {
            add
            {
                if (_tickArrivedEvent == null)
                {
                    _tickArrivedEvent += value;
                }
            }
            remove { _tickArrivedEvent -= value; }
        }

        public event Action<BarDetail> BarArrivedEvent
        {
            add
            {
                if (_barArrivedEvent == null)
                {
                    _barArrivedEvent += value;
                }
            }
            remove { _barArrivedEvent -= value; }
        }

        public event Action<HistoricBarData> HistoricalDataArrivedEvent
        {
            add
            {
                if (_historicalDataArrivedEvent == null)
                {
                    _historicalDataArrivedEvent += value;
                }
            }
            remove { _historicalDataArrivedEvent -= value; }
        }

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="marketDataService">Provides communication access with Market Data Server for live data</param>
        /// <param name="historicalDataService">Provider communication access with Market Data for historical data</param>
        public MarketDataManager(MarketDataService marketDataService, HistoricalDataService historicalDataService)
        {
            // Save Instance
            _marketDataService = marketDataService;
            _historicalDataService = historicalDataService;

            // Initialize
            _idGenerator = new MarketDataIdGenerator();
            _barParametersMap = new Dictionary<string, BarParameters>();

            SubscribeDataServiceEvents();

            //_marketDataService.StartService();
            //_historicalDataService.StartService();
        }

        /// <summary>
        /// Register Market Data Service Events
        /// </summary>
        private void SubscribeDataServiceEvents()
        {
            // Makes sure that events are only hooked once
            UnsubscribeDataServiceEvents();

            _marketDataService.Connected += OnConnected;
            _marketDataService.Disconnected += OnDisconnected;

            _marketDataService.LogonArrived += OnLogonArrived;
            _marketDataService.LogoutArrived += OnLogoutArrived;
            
            _marketDataService.TickArrived += OnTickArrived;
            _marketDataService.BarArrived += OnBarArrived;
            _historicalDataService.HistoricalDataArrived += OnHistoricalDataArrived;
        }

        /// <summary>
        /// Unsubscribe Market Data Service Events
        /// </summary>
        private void UnsubscribeDataServiceEvents()
        {
            _marketDataService.Connected -= OnConnected;
            _marketDataService.Disconnected -= OnDisconnected;

            _marketDataService.LogonArrived -= OnLogonArrived;
            _marketDataService.LogoutArrived -= OnLogoutArrived;

            _marketDataService.TickArrived -= OnTickArrived;
            _marketDataService.BarArrived -= OnBarArrived;
        }

        #region Connect/Disconnect

        /// <summary>
        /// Establishes connection with Market Data Server
        /// </summary>
        public void Connect()
        {
            // Initialize services to re-establish connection
            _marketDataService.InitializeService();
            _historicalDataService.InitializeService();

            // Start Services
            _marketDataService.StartService();
            _historicalDataService.StartService();
        }

        /// <summary>
        /// Terminates connection with Market Data Server
        /// </summary>
        public void Disconnect()
        {
            _marketDataService.StopService();
            _historicalDataService.StopService();
        }

        /// <summary>
        /// Sends request to Market Data Server to connect given market data provider
        /// </summary>
        /// <param name="providerName">Market Data Provider to connect</param>
        public void Connect(string providerName)
        {
            // Create a new login message
            Login login = new Login()
            {
                MarketDataProvider = providerName
            };

            _marketDataService.Login(login);
        }

        /// <summary>
        /// Sends request to Market Data Server for disconnecting given market data provider
        /// </summary>
        /// <param name="providerName">Market Data Provider to disconnect</param>
        public void Disconnect(string providerName)
        {
            // Unsubscribe all Symbols before sending logout.
            _marketDataService.UnsubscribeAllSecurities(providerName);

            // Unsubscribe all bar data before sending logout
            _marketDataService.UnsubscribeAllLiveBars(providerName);

            // Create a new logout message
            Logout logout = new Logout()
            {
                MarketDataProvider = providerName
            };

            _marketDataService.Logout(logout);
            _historicalDataService.Logout(logout);
        }

        #endregion

        #region Subscribe/Unsubscribe

        /// <summary>
        /// Sends subscription request to Market Data Server
        /// </summary>
        /// <param name="security">Contains symbol information</param>
        /// <param name="providerName">Name of the provider on which to subscribe</param>
        public void Subscribe(Security security, string providerName)
        {
            // Create subscription message
            Subscribe subscribe = SubscriptionMessage.TickSubscription(_idGenerator.NextTickId(), security, providerName);

            _marketDataService.Subscribe(subscribe);
        }

        /// <summary>
        /// Sends bar subscription request to Market Data Server
        /// </summary>
        /// <param name="security">Contains symbol information</param>
        /// <param name="barDetail">Contains parameter information for the bar to be subscribed</param>
        /// <param name="providerName">Name of the provider on which to subscribe</param>
        public void SubscribeBar(Security security, BarParameters barDetail, string providerName)
        {
            // Create bar subscription message
            BarDataRequest subscribe = SubscriptionMessage.LiveBarSubscription(_idGenerator.NextBarId(), security,
                barDetail.Format, barDetail.PriceType, barDetail.BarLength, barDetail.PipSize, 0, providerName);

            // Add information to local map
            _barParametersMap.Add(subscribe.Id, barDetail);

            _marketDataService.Subscribe(subscribe);
        }

        /// <summary>
        /// Sends Historical bar data request to Market Data Server
        /// </summary>
        /// <param name="security">Contains symbol information</param>
        /// <param name="barDetail">Contains parameter information for the historical bars to be fetched</param>
        /// <param name="providerName">Name of the provider on which to subscribe</param>
        public void SubscribeHistoricalData(Security security, HistoricalBarParameters barDetail, string providerName)
        {
            // Create bar subscription message
            HistoricDataRequest subscribe =
                SubscriptionMessage.HistoricDataSubscription(_idGenerator.NextHistoricalDataId(), security,
                    barDetail.StartDate, barDetail.EndDate, barDetail.Interval, barDetail.Type, providerName);

            _historicalDataService.Subscribe(subscribe);
        }

        /// <summary>
        /// Sends un-subscription request to Market Data Server
        /// </summary>
        /// <param name="security">Contains symbol information</param>
        /// <param name="providerName">Name of the provider on which to unsubscribe</param>
        public void Unsubscribe(Security security, string providerName)
        {
            // Create unsubscription message
            Unsubscribe unsubscribe = SubscriptionMessage.TickUnsubscription("", security, providerName);

            _marketDataService.Unsubscribe(unsubscribe);
        }

        /// <summary>
        /// Sends bar un-subscription request to Market Data Server
        /// </summary>
        /// <param name="security">Contains symbol information</param>
        /// <param name="barDetail">Contains parameter information for the bar to be subscribed</param>
        /// <param name="providerName">Name of the provider on which to subscribe</param>
        public void UnsubscribeBar(Security security, BarParameters barDetail, string providerName)
        {
            // Create bar un-subscription message
            BarDataRequest unsubscribe = SubscriptionMessage.LiveBarUnsubscription(_idGenerator.NextBarId(), security,
                barDetail.Format, barDetail.PriceType, barDetail.BarLength, barDetail.PipSize, 0, providerName);

            _marketDataService.Unsubscribe(unsubscribe);
        }

        #endregion

        #region Market Data Service Events

        /// <summary>
        /// Called when client is connected to Server
        /// </summary>
        private void OnConnected()
        {
            if (_connectedEvent!=null)
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
        /// Called when requested market data provider is successfully 'Logged IN'
        /// </summary>
        /// <param name="providerName">Market Data Provider name</param>
        private void OnLogonArrived(string providerName)
        {
            if (_logonArrivedEvent != null)
            {
                _logonArrivedEvent(providerName);
            }
        }

        /// <summary>
        /// Called when requested market data provider is successfully 'Logged OUT'
        /// </summary>
        /// <param name="providerName">Narket Data Provider name</param>
        private void OnLogoutArrived(string providerName)
        {
            if (_logoutArrivedEvent != null)
            {
                _logoutArrivedEvent(providerName);
            }
        }

        /// <summary>
        /// Called when new Tick information is received from Market Data Server
        /// </summary>
        /// <param name="tick">Contains market details</param>
        private void OnTickArrived(Tick tick)
        {
            if (_tickArrivedEvent != null)
            {
                _tickArrivedEvent(tick);
            }
        }

        /// <summary>
        /// Called when new Bar information is received from Market Data Server
        /// </summary>
        /// <param name="bar">Contains bar details</param>
        private void OnBarArrived(Bar bar)
        {
            if (_barArrivedEvent != null)
            {
                BarParameters barParameters;
                if (_barParametersMap.TryGetValue(bar.RequestId, out barParameters))
                {
                    // Create new detail object
                    BarDetail barDetail = new BarDetail(bar, barParameters);

                    _barArrivedEvent(barDetail);   
                }
            }
        }

        /// <summary>
        /// Called when Historical Bar data is received from Market Data Server
        /// </summary>
        /// <param name="historicBarData">Contains requested Historical data details</param>
        private void OnHistoricalDataArrived(HistoricBarData historicBarData)
        {
            if (_historicalDataArrivedEvent != null)
            {
                _historicalDataArrivedEvent(historicBarData);
            }
        }

        #endregion

        /// <summary>
        /// Stops all market data activites and closes open connections
        /// </summary>
        public void Stop()
        {
            // Unsubscribe all existing securities
            _marketDataService.UnsubscribeAllSecurities();

            // Unsubscribe all bars
            _marketDataService.UnsubscribeAllLiveBars();

            // Stop Service
            _marketDataService.StopService();
            _historicalDataService.StopService();
        }
    }
}
