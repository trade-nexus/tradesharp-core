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
using System.Windows.Threading;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.StrategyEngine.HistoricalData;
using TradeHub.StrategyEngine.MarketData;
using TradeSharp.ServiceControllers.Managers;
using TradeSharp.UI.Common;
using TradeSharp.UI.Common.Constants;
using TradeSharp.UI.Common.Models;
using TradeSharp.UI.Common.Utility;
using TradeSharp.UI.Common.ValueObjects;
using MarketDataProvider = TradeSharp.UI.Common.Models.MarketDataProvider;

namespace TradeSharp.ServiceControllers.Services
{
    /// <summary>
    /// Provides access for Market Data related queries and response
    /// </summary>
    public class MarketDataController
    {
        private Type _type = typeof (MarketDataController);

        /// <summary>
        /// Holds UI thread reference
        /// </summary>
        private Dispatcher _currentDispatcher;

        /// <summary>
        /// Responsible for providing requested market data functionality
        /// </summary>
        private MarketDataManager _marketDataManager;

        /// <summary>
        /// Keeps tracks of all the Providers
        /// KEY = Provider Name
        /// Value = Provider details <see cref="Provider"/>
        /// </summary>
        private IDictionary<string, MarketDataProvider> _providersMap;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="marketDataService">Provides communication access with Market Data Server for live data</param>
        /// <param name="historicalDataService">Provides communication access with Market Data Server for historical data</param>
        public MarketDataController(MarketDataService marketDataService, HistoricalDataService historicalDataService)
        {
            this._currentDispatcher = Dispatcher.CurrentDispatcher;

            // Initialize Manager
            _marketDataManager = new MarketDataManager(marketDataService, historicalDataService);

            // Intialize local maps
            _providersMap = new Dictionary<string, MarketDataProvider>();

            // Subscribe Application events
            SubscribeEvents();

            // Subscribe Market Data Manager events
            SubscribeManagerEvents();
        }

        /// <summary>
        /// Subscribe events to receive incoming market data requests
        /// </summary>
        private void SubscribeEvents()
        {
            // Register Event to receive connect/disconnect requests
            EventSystem.Subscribe<MarketDataProvider>(NewConnectionRequest);

            // Register Event to receive subscribe/unsubscribe requests
            EventSystem.Subscribe<SubscriptionRequest>(NewSubscriptionRequest);

            // Register Event to receive service notifications
            EventSystem.Subscribe<ServiceDetails>(OnServiceStatusModification);
        }
        
        /// <summary>
        /// Subscribe events to receive incoming data and responses from Market Data Manager
        /// </summary>
        private void SubscribeManagerEvents()
        {
            _marketDataManager.LogonArrivedEvent += OnLogonArrived;
            _marketDataManager.LogoutArrivedEvent += OnLogoutArrived;

            _marketDataManager.TickArrivedEvent += OnTickArrived;
            _marketDataManager.BarArrivedEvent += OnBarArrived;
            _marketDataManager.HistoricalDataArrivedEvent += OnHistoricalDataArrived;
        }

        #region Incoming Requests

        /// <summary>
        /// Called when new Connection request is made by the user
        /// </summary>
        /// <param name="marketDataProvider"></param>
        private void NewConnectionRequest(MarketDataProvider marketDataProvider)
        {
            // Only entertain 'Market Data Provider' related calls
            if (!marketDataProvider.ProviderType.Equals(ProviderType.MarketData))
                return;

            if (marketDataProvider.ConnectionStatus.Equals(ConnectionStatus.Disconnected))
            {
                // Open a new market data connection
                ConnectMarketDataProvider(marketDataProvider);
            }
            else if (marketDataProvider.ConnectionStatus.Equals(ConnectionStatus.Connected))
            {
                // Close existing connection
                DisconnectMarketDataProvider(marketDataProvider);
            }
        }

        /// <summary>
        /// Called when a new subscription request is made by the user
        /// </summary>
        /// <param name="subscriptionRequest"></param>
        private void NewSubscriptionRequest(SubscriptionRequest subscriptionRequest)
        {
            if (subscriptionRequest.SubscriptionType.Equals(SubscriptionType.Subscribe))
            {
                Subscribe(subscriptionRequest);
            }
            else
            {
                Unsubscribe(subscriptionRequest);
            }
        }

        /// <summary>
        /// Called when connection request is received for given Market Data Provider
        /// </summary>
        /// <param name="marketDataProvider">Contains provider details</param>
        private void ConnectMarketDataProvider(MarketDataProvider marketDataProvider)
        {
            // Check if the provider already exists in the local map
            if (!_providersMap.ContainsKey(marketDataProvider.ProviderName))
            {
                // Add incoming provider to local map
                _providersMap.Add(marketDataProvider.ProviderName, marketDataProvider);
            }

            // Check current provider status
            if (marketDataProvider.ConnectionStatus.Equals(ConnectionStatus.Disconnected))
            {
                // Forward connection request
                _marketDataManager.Connect(marketDataProvider.ProviderName);
            }
            else
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(marketDataProvider.ProviderName + " connection status is already set to connected.",
                        _type.FullName, "ConnectMarketDataProvider");
                }
            }
        }

        /// <summary>
        /// Called when disconnect request is received for given Market Data Provider
        /// </summary>
        /// <param name="marketDataProvider">Contains provider details</param>
        private void DisconnectMarketDataProvider(MarketDataProvider marketDataProvider)
        {
            // Check current provider status
            if (marketDataProvider.ConnectionStatus.Equals(ConnectionStatus.Connected))
            {
                // Forward disconnect request
                _marketDataManager.Disconnect(marketDataProvider.ProviderName);
            }
            else
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(marketDataProvider.ProviderName + " connection status is already set to dis-connected.",
                        _type.FullName, "DisconnectMarketDataProvider");
                }
            }
        }

        /// <summary>
        /// Called when your user requests new market data subscription
        /// </summary>
        /// <param name="subscriptionRequest">Contains subscription information</param>
        private void Subscribe(SubscriptionRequest subscriptionRequest)
        {
            // Send Tick data subscription request
            if (subscriptionRequest.MarketDataType.Equals(MarketDataType.Tick))
            {
                _marketDataManager.Subscribe(subscriptionRequest.Security, subscriptionRequest.Provider.ProviderName);
            }
            // Send Bar data subscription request
            else if (subscriptionRequest.MarketDataType.Equals(MarketDataType.Bar))
            {
                _marketDataManager.SubscribeBar(subscriptionRequest.Security, subscriptionRequest.LiveBarDetail, subscriptionRequest.Provider.ProviderName);
            }
            // Send Historical data request
            else if (subscriptionRequest.MarketDataType.Equals(MarketDataType.Historical))
            {
                _marketDataManager.SubscribeHistoricalData(subscriptionRequest.Security,subscriptionRequest.HistoricalBarDetail, subscriptionRequest.Provider.ProviderName);
            }
        }

        /// <summary>
        /// Called when your user requests market data to be unsubscribed
        /// </summary>
        /// <param name="subscriptionRequest">Contains subscription information</param>
        private void Unsubscribe(SubscriptionRequest subscriptionRequest)
        {
            _marketDataManager.Unsubscribe(subscriptionRequest.Security, subscriptionRequest.Provider.ProviderName);
        }

        #endregion

        #region Market Data Manager Events

        /// <summary>
        /// Called when requested provider is successfully 'Logged ON'
        /// </summary>
        /// <param name="providerName"></param>
        private void OnLogonArrived(string providerName)
        {
            MarketDataProvider provider;
            if (_providersMap.TryGetValue(providerName, out provider))
            {
                provider.ConnectionStatus = ConnectionStatus.Connected;

                // Raise event to update UI
                EventSystem.Publish<UiElement>(new UiElement());
            }
        }

        /// <summary>
        /// Called when requested market data provider is successfully 'Logged OUT'
        /// </summary>
        /// <param name="providerName"></param>
        private void OnLogoutArrived(string providerName)
        {
            MarketDataProvider provider;
            if (_providersMap.TryGetValue(providerName, out provider))
            {
                provider.ConnectionStatus = ConnectionStatus.Disconnected;

                // Raise event to update UI
                EventSystem.Publish<UiElement>(new UiElement());
            }
        }

        /// <summary>
        /// Called when new Tick information is received from Market Data Sever
        /// </summary>
        /// <param name="tick">Contains market details</param>
        private void OnTickArrived(Tick tick)
        {
            MarketDataProvider provider;

            // Get Provider object
            if (_providersMap.TryGetValue(tick.MarketDataProvider, out provider))
            {
                provider.UpdateMarketDetail(tick.Security.Symbol, tick);

                if (provider.IsQuotePersistenceRequired(tick.Security.Symbol)
                    || provider.IsTradePersistenceRequired(tick.Security.Symbol))
                {
                    EventSystem.Publish<Tick>(tick);
                }
            }
        }

        /// <summary>
        /// Called when new Bar is received from Market Data Server
        /// </summary>
        /// <param name="barDetail"></param>
        private void OnBarArrived(BarDetail barDetail)
        {
            MarketDataProvider provider;

            // Get Provider object
            if (_providersMap.TryGetValue(barDetail.Bar.MarketDataProvider, out provider))
            {
                if (provider.IsBarPersistenceRequired(barDetail.Bar.Security.Symbol))
                {
                    EventSystem.Publish<BarDetail>(barDetail);   
                }
            }
        }

        /// <summary>
        /// Called when request historical bar data is received from Market Data Server
        /// </summary>
        /// <param name="historicBarData"></param>
        private void OnHistoricalDataArrived(HistoricBarData historicBarData)
        {
            EventSystem.Publish<HistoricBarData>(historicBarData);
        }

        #endregion

        /// <summary>
        /// Called when Service status is modified
        /// </summary>
        /// <param name="serviceDetails"></param>
        private void OnServiceStatusModification(ServiceDetails serviceDetails)
        {
            if (serviceDetails.ServiceName.Equals(GetEnumDescription.GetValue(TradeSharp.UI.Common.Constants.Services.MarketDataService)))
            {
                if (serviceDetails.Status.Equals(ServiceStatus.Running))
                {
                    _marketDataManager.Connect();
                }
                else if (serviceDetails.Status.Equals(ServiceStatus.Stopped))
                {
                    _marketDataManager.Disconnect();
                }
            }
        }

        /// <summary>
        /// Stops all market data related activities
        /// </summary>
        public void Stop()
        {
            // Send logout for each connected market data provider
            foreach (KeyValuePair<string, MarketDataProvider> keyValuePair in _providersMap)
            {
                if (keyValuePair.Value.ConnectionStatus.Equals(ConnectionStatus.Connected))
                {
                    _marketDataManager.Disconnect(keyValuePair.Key);
                }
            }

            _marketDataManager.Stop();
        }
    }
}
