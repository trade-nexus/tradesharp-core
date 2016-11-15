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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.MarketDataProvider;
using TradeHub.Common.Core.Utility;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataProvider.IqFeed.ValueObject;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.MarketDataProvider.IqFeed.Provider
{
    public class IqFeedMarketDataProvider : ILiveTickDataProvider, ILiveBarDataProvider, IHistoricBarDataProvider
    {
        private Type _type = typeof (IqFeedMarketDataProvider);

        private AsyncClassLogger _logger;
        
        /// <summary>
        /// Indicates if the Provider session is connected or not
        /// </summary>
        private bool _isConnected;

        /// <summary>
        /// Contains Provider name used through out TradeSharp
        /// </summary>
        private readonly string _marketDataProviderName;

        /// <summary>
        /// Contains connection details required for communication
        /// </summary>
        private ConnectionParameters _connectionParameters;

        #region Events

        /// <summary>
        /// Fired each time a Logon is arrived
        /// </summary>
        public event Action<string> LogonArrived;

        /// <summary>
        /// Fired each time a Logout is arrived
        /// </summary>
        public event Action<string> LogoutArrived;

        /// <summary>
        /// Fired each time a new tick arrives.
        /// </summary>
        public event Action<Tick> TickArrived;

        /// <summary>
        /// Fired each time a new Bar Arrives
        /// Bar =  TradeHub Bar Object
        /// String =  Request ID
        /// </summary>
        public event Action<Bar, string> BarArrived;

        /// <summary>
        /// Fired when requested Historic Bar Data arrives
        /// </summary>
        public event Action<HistoricBarData> HistoricBarDataArrived;

        /// <summary>
        /// Fired each time when market data rejection arrives.
        /// </summary>
        public event Action<MarketDataEvent> MarketDataRejectionArrived;

        #endregion

        /// <summary>
        /// Provides Level 1 data from IQ Feed
        /// </summary>
        private LevelOneData _levelOneData;

        /// <summary>
        /// Provides Live Bars from IQ Feed
        /// </summary>
        private BarData _barData;

        /// <summary>
        /// Provides Historical Bar Data from IQ Feed
        /// </summary>
        private HistoricalData _historicalData;

        /// <summary>
        /// Responsible for connecting to local IQ Feed Connector application
        /// </summary>
        private ConnectionForm _connectionForm;

        public IqFeedMarketDataProvider()
        {
            // Create object for logging details
            _logger = new AsyncClassLogger("IqFeedDataProvider");
            _logger.SetLoggingLevel();
            _logger.LogDirectory(Constants.DirectoryStructure.MDE_LOGS_LOCATION);

            // Set provider name
            _marketDataProviderName = Constants.MarketDataProvider.IqFeed;

            // Object will be used for connecting to local IQ Feed Connector application
            _connectionForm = new ConnectionForm(_logger);

            // Initialize Data classes
            _levelOneData = new LevelOneData(_logger);
            _barData = new BarData(_logger);
            _historicalData = new HistoricalData(_logger);

            // Register local events
            SubscribeLocalDataEvents();
        }

        /// <summary>
        /// Registers Events from local data classes
        /// </summary>
        private void SubscribeLocalDataEvents()
        {
            // Makes sure events are subscribed only once
            UnsubscribeLocalDataEvents();

            _levelOneData.ConnectionEvent += OnDataFeedConnected;

            _levelOneData.DataEvent += OnLevelOneDataArrived;
            _barData.BarDataEvent += OnLiveBarArrived;
            _historicalData.HistoricalDataEvent += OnHistoricalDataArrived;
        }

        /// <summary>
        /// Unsubscribes Events from local data classes
        /// </summary>
        private void UnsubscribeLocalDataEvents()
        {
            _levelOneData.ConnectionEvent -= OnDataFeedConnected;

            _levelOneData.DataEvent -= OnLevelOneDataArrived;
            _barData.BarDataEvent -= OnLiveBarArrived;
            _historicalData.HistoricalDataEvent -= OnHistoricalDataArrived;
        }

        #region Connection Methods

        /// <summary>
        /// Indicates if the Provider session is connected or not
        /// </summary>
        public bool IsConnected()
        {
            return _isConnected;
        }

        /// <summary>
        /// Connects/Starts a client
        /// </summary>
        public bool Start()
        {
            try
            {
                // Read account credentials
                ReadConnectionParameters();

                if (_connectionParameters == null)
                {
                    _logger.Info("Connection Parameters unavailable", _type.FullName, "Start");
                 
                    return false;
                }

                // Send credential details to connection form
                //_connectionForm.Connect(_connectionParameters.LoginId, _connectionParameters.Password,
                //    _connectionParameters.ProductId, _connectionParameters.ProductVersion);

                //Thread.Sleep(4000);

                // Open connection with Data feed
                _barData.OpenBarDataConnection();
                _historicalData.OpenHistoricalDataConnection();
                _levelOneData.OpenLevelOneDataConnection();

                return true;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "Start");
            }
            return false;
        }

        /// <summary>
        /// Disconnects/Stops a client
        /// </summary>
        public bool Stop()
        {
            try
            {
                if (_isConnected)
                {
                    // Stop subscribed data feed
                    _levelOneData.Stop();
                    _barData.Stop();

                    //// Close connection with the local IQFeed application
                    //_connectionForm.Stop();

                    _isConnected = false;

                    // Raise event to notify listeners
                    if (LogoutArrived != null)
                    {
                        LogoutArrived(_marketDataProviderName);
                    }

                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "Stop");
                return false;
            }
        }

        #endregion

        #region Subscription

        /// <summary>
        /// Market data request message
        /// </summary>
        public bool SubscribeTickData(Subscribe request)
        {
            try
            {
                if (_isConnected)
                {
                    // Forward request
                    _levelOneData.Subscribe(request.Security.Symbol);

                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "SubscribeTickData");
                return false;
            }
        }

        /// <summary>
        /// Unsubscribe Market data message
        /// </summary>
        public bool UnsubscribeTickData(Unsubscribe request)
        {
            try
            {
                if (_isConnected)
                {
                    // Forward request
                    _levelOneData.Unsubscribe(request.Security.Symbol);

                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "UnsubscribeTickData");
                return false;
            }
        }

        /// <summary>
        /// Request to get Bar Data
        /// </summary>
        /// <param name="barDataRequest">TradeHub Bar Data Request Message</param>
        /// <returns></returns>
        public bool SubscribeBars(BarDataRequest barDataRequest)
        {
            try
            {
                if (_isConnected)
                {
                    // Forward request
                    _barData.Subscribe(barDataRequest);

                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "SubscribeBars");
                return false;
            }
        }

        /// <summary>
        /// Unsubscribe Bar data
        /// </summary>
        /// <param name="barDataRequest">TradeHub Bar Data Request Message</param>
        /// <returns></returns>
        public bool UnsubscribeBars(BarDataRequest barDataRequest)
        {
            try
            {
                if (_isConnected)
                {
                    // Forward request
                    _barData.Unsubscribe(barDataRequest);

                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "UnsubscribeBars");
                return false;
            }
        }

        /// <summary>
        /// Historic Bar Data Request Message
        /// </summary>
        public bool HistoricBarDataRequest(HistoricDataRequest historicDataRequest)
        {
            try
            {
                if (_isConnected)
                {
                    // Forward request
                    _historicalData.Subscribe(historicDataRequest);

                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "HistoricBarDataRequest");
                return false;
            }
        }


        #endregion

        /// <summary>
        /// Reads required connections pamareters from stored file
        /// </summary>
        private void ReadConnectionParameters()
        {
            // Read parameters from given file
            var parameters = ParameterReader.ReadParamters("IqFeedParams.xml", "IqFeed");

            if (parameters.Count == 4)
            {
                string loginId;
                parameters.TryGetValue("LoginID", out loginId);

                string password;
                parameters.TryGetValue("Password", out password);

                string productionId;
                parameters.TryGetValue("ProductID", out productionId);

                string productVersion;
                parameters.TryGetValue("ProductVersion", out productVersion);

                // Create new object
                _connectionParameters = new ConnectionParameters(loginId, password, productionId, productVersion);

                return;
            }

            _connectionParameters = null;
        }

        /// <summary>
        /// Called when IQ Data Feed server status changes
        /// </summary>
        /// <param name="status"></param>
        private void OnDataFeedConnected(bool status)
        {
            _isConnected = status;

            if (_logger.IsInfoEnabled)
            {
                _logger.Info("Data Feed Server connected: " + _isConnected, _type.FullName, "OnDataFeedConnected");
            }

            // Raise event to notify listeners
            if (LogonArrived != null)
            {
                LogonArrived(_marketDataProviderName);
            }
        }

        /// <summary>
        /// Called when new Tick data is received
        /// </summary>
        /// <param name="tick"></param>
        private void OnLevelOneDataArrived(Tick tick)
        {
            // Raise Event to notify listeners
            if (TickArrived != null)
            {
                TickArrived(tick);
            }
        }

        /// <summary>
        /// Called when new live Bar is received
        /// </summary>
        /// <param name="bar"></param>
        private void OnLiveBarArrived(Bar bar)
        {
            // Raise Event to notify listeners
            if (BarArrived != null)
            {
                BarArrived(bar, bar.RequestId);
            }
        }

        /// <summary>
        /// Called when requested Historical data is received
        /// </summary>
        /// <param name="historicBarData"></param>
        private void OnHistoricalDataArrived(HistoricBarData historicBarData)
        {
            // Raise Event to notify listeners
            if (HistoricBarDataArrived != null)
            {
                HistoricBarDataArrived(historicBarData);
            }
        }
    }
}
