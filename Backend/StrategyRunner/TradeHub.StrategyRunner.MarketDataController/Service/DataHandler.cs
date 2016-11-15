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
using System.Threading.Tasks;
using Disruptor;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.StrategyRunner.Infrastructure.ValueObjects;
using TradeHub.StrategyRunner.MarketDataController.Utility;

namespace TradeHub.StrategyRunner.MarketDataController.Service
{
    /// <summary>
    /// Responsible for managing all Market Data requests and providing appropriate responses
    /// </summary>
    public class DataHandler : IEventHandler<MarketDataObject>
    {
        private Type _type = typeof(DataHandler);
        private AsyncClassLogger _classLogger;

        public event Action<Tick> TickReceived;
        public event Action<Bar> BarReceived;
        public event Action<HistoricBarData> HistoricDataReceived;

        public bool ConnectionStatus { get; set; }

        /// <summary>
        /// Will contain all the symbols for which bar is subscribed
        /// </summary>
        public IList<string> BarSubscriptionList
        {
            get { return _barSubscriptionList; }
            set { _barSubscriptionList = value; }
        }

        /// <summary>
        /// Will contain all the symbols for which tick is subscribed
        /// </summary>
        public IList<string> TickSubscriptionList
        {
            get { return _tickSubscriptionList; }
            set { _tickSubscriptionList = value; }
        }

        private FetchData _fetchMarketData;

        /// <summary>
        /// Will contain all the symbols for which bar is subscribed
        /// </summary>
        private IList<string> _barSubscriptionList;

        /// <summary>
        /// Will contain all the symbols for which tick is subscribed
        /// </summary>
        private IList<string> _tickSubscriptionList;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DataHandler()
        {
            try
            {
                _classLogger = new AsyncClassLogger("DataHandler");
                _classLogger.SetLoggingLevel();
                //set logging path
                string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                             "\\TradeHub Logs\\Client";
                _classLogger.LogDirectory(path);

                _fetchMarketData = new FetchData(new ReadMarketData(_classLogger), _classLogger);

                // Initialize Lists
                BarSubscriptionList = new List<string>();
                TickSubscriptionList = new List<string>();

                _fetchMarketData.InitializeDisruptor(new IEventHandler<MarketDataObject>[] {this});
                _fetchMarketData.HistoricalDataFired += HistoricDataArrived;
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "DataHandler");
            }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="eventHandler">Event handler which will receive requested data</param>
        public DataHandler(IEventHandler<MarketDataObject>[] eventHandler)
        {
            try
            {
                _classLogger = new AsyncClassLogger("DataHandler");
                _classLogger.SetLoggingLevel();

                //set logging path
                string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                             "\\TradeHub Logs\\Client";
                _classLogger.LogDirectory(path);

                _fetchMarketData = new FetchData(new ReadMarketData(_classLogger), _classLogger);

                // Initialize Lists
                BarSubscriptionList = new List<string>();
                TickSubscriptionList = new List<string>();

                _fetchMarketData.InitializeDisruptor(eventHandler);
                _fetchMarketData.HistoricalDataFired += HistoricDataArrived;
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "DataHandler");
            }
        }

        /// <summary>
        /// Subscribe to New Symbol for bar data
        /// </summary>
        /// <param name="barDataRequest"></param>
        public void SubscribeSymbol(BarDataRequest barDataRequest)
        {
            if (_classLogger.IsInfoEnabled)
            {
                _classLogger.Info("New subscription request recieved Request for " + barDataRequest, _type.FullName, "SubscribeSymbol");
            }

            // Add new symbol to the Bar list
            if (!BarSubscriptionList.Contains(barDataRequest.Security.Symbol))
            {
                BarSubscriptionList.Add(barDataRequest.Security.Symbol);
            }

            // Fetch data if its not already fetched for ticks
            if (!TickSubscriptionList.Contains(barDataRequest.Security.Symbol))
            {
                FetchData(barDataRequest);
            }
        }

        /// <summary>
        /// Subscribes Tick data for the given symbol
        /// </summary>
        /// <param name="subscribe">Contains info for the symbol to be subscribed</param>
        public void SubscribeSymbol(Subscribe subscribe)
        {
            try
            {
                if (_classLogger.IsInfoEnabled)
                {
                    _classLogger.Info("New subscription request received " + subscribe, _type.FullName, "SubscribeSymbol");
                }

                // Add new symbol to the Tick list
                if (!TickSubscriptionList.Contains(subscribe.Security.Symbol))
                {
                    TickSubscriptionList.Add(subscribe.Security.Symbol);
                }

                // Fetch data if its not already fetched for bars
                if(!BarSubscriptionList.Contains(subscribe.Security.Symbol))
                {
                    FetchData(subscribe);
                }
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "SubscribeSymbol");
            }
        }

        /// <summary>
        /// Subscribe to New Symbol for Historical data
        /// </summary>
        /// <param name="historicDataRequest"></param>
        public void SubscribeSymbol(HistoricDataRequest historicDataRequest)
        {
            if (_classLogger.IsInfoEnabled)
            {
                _classLogger.Info("New subscription request received for " + historicDataRequest, _type.FullName, "SubscribeSymbol");
            }
            FetchData(historicDataRequest);
        }

        /// <summary>
        /// Fire Historical Bar Data
        /// </summary>
        /// <param name="historicBarData">TradeHub HistoricalBarData contains requested historical bars</param>
        private void HistoricDataArrived(HistoricBarData historicBarData)
        {
            try
            {
                if (HistoricDataReceived != null)
                {
                    HistoricDataReceived(historicBarData);
                }

                if (_classLogger.IsDebugEnabled)
                {
                    _classLogger.Debug(historicBarData.ToString(), _type.FullName, "HistoricDataArrived");
                }
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "HistoricDataArrived");
            }
        }

        /// <summary>
        /// Creats a seprate thread for each request.
        /// </summary>
        /// <param name="request"></param>
        public void FetchData(BarDataRequest request)
        {
            try
            {
                Task.Factory.StartNew(() => _fetchMarketData.ReadData(request));
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "FetchData");
            }
        }

        /// <summary>
        /// Fetches data for the required symbol from stored files
        /// </summary>
        /// <param name="subscribe">Contains info for the subscribing symbol</param>
        private void FetchData(Subscribe subscribe)
        {
            try
            {
                Task.Factory.StartNew(() => _fetchMarketData.ReadData(subscribe));
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "FetchData");
            }
        }

        /// <summary>
        /// Fetches data for the required symbol from stored files
        /// </summary>
        /// <param name="historicDataRequest">Contains historical request info for subscribing symbol</param>
        private void FetchData(HistoricDataRequest historicDataRequest)
        {
            try
            {
                Task.Factory.StartNew(() => _fetchMarketData.ReadData(historicDataRequest));
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "FetchData");
            }
        }

        /// <summary>
        /// Called when Bar/Tick data is completely sent
        /// </summary>
        /// <param name="message"></param>
        private void OnDataCompleted(string message)
        {
            try
            {
                // NOTE: Commented out because Disruptor is still sending data while this event is raised.
                if(message.Contains("DataCompleted"))
                {
                    //var info = message.Split(',');

                    //if (_barSubscriptionList.Contains(info[1]))
                    //    _barSubscriptionList.Remove(info[1]);

                    //if (_tickSubscriptionList.Contains(info[1]))
                    //    _tickSubscriptionList.Remove(info[1]);
                }
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "OnDataCompleted");
            }
        }

        /// <summary>
        /// Close active connections/services
        /// </summary>
        public void Shutdown()
        {
            _fetchMarketData.ShutdownDisruptor();
        }

        #region Implementation of IEventHandler<in MarketDataObject>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(MarketDataObject data, long sequence, bool endOfBatch)
        {
            if (data.IsTick)
            {
                // Publish Tick if the subscription request is received
                if (TickSubscriptionList.Contains(data.Tick.Security.Symbol))
                {
                    // Raise Event to notify listeners
                    if (TickReceived != null)
                        TickReceived(data.Tick);

                    if (_classLogger.IsDebugEnabled)
                    {
                        _classLogger.Debug(data.Tick.ToString(), _type.FullName, "OnNext");
                    }
                }
            }
            else
            {
                // Publish Bar if the subscription request is received
                if (BarSubscriptionList.Contains(data.Bar.Security.Symbol))
                {
                    // Raise Event to notify listeners
                    if (BarReceived != null)
                        BarReceived(data.Bar);

                    if (_classLogger.IsDebugEnabled)
                    {
                        _classLogger.Debug(data.Bar.ToString(), _type.FullName, "OnNext");
                    }
                }
            }
        }

        #endregion
    }
}
