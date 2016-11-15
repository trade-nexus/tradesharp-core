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
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using Disruptor;
using Disruptor.Dsl;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.SimulatedExchange.Common;
using TradeHub.SimulatedExchange.Common.Interfaces;
using TradeHub.SimulatedExchange.Common.ValueObjects;
using TradeHub.SimulatedExchange.DomainObjects;
using TradeHub.SimulatedExchange.DomainObjects.Constant;
using TradeHub.SimulatedExchange.FileReader;

namespace TradeHub.SimulatedExchange.SimulatorControler
{
    public class MarketDataControler : IEventHandler<MarketDataObject>, IMarketDataControler
    {
        private Type _type = typeof(MarketDataControler);

        /// <summary>
        /// Responsibe for all communication with the requesting party
        /// </summary>
        private readonly ICommunicationController _communicationController;

        public event Action<Bar,string> BarArrived;
        public bool ConnectionStatus { get; set; }
        public FetchData FetchMarketData;

        /// <summary>
        /// Will contain all the symbols for which bar is subscribed
        /// </summary>
        private IList<string> _barSubscriptionList;

        /// <summary>
        /// Will contain all the symbols for which tick is subscribed
        /// </summary>
        private IList<string> _tickSubscriptionList;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="fetchData"></param>
        /// <param name="communicationController"></param>
        public MarketDataControler(FetchData fetchData,ICommunicationController communicationController)
        {
            try
            {
                Logger.SetLoggingLevel();

                // Initialize Lists
                _barSubscriptionList = new List<string>();
                _tickSubscriptionList = new List<string>();

                _communicationController = communicationController;
                SubscribeRequiredEvents();
                FetchMarketData = fetchData;

                FetchMarketData.BarFired += FetchMarketDataBarFired;
                FetchMarketData.TickFired += FetchMarketDataTickFired;
                FetchMarketData.HistoricalDataFired += FetchMarketDataHistoricDataFired;

                EventSystem.Subscribe<string>(OnDataCompleted);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "MarketDataControler");
            }
        }

        /// <summary>
        /// Establist Connection with Simulated Exchange
        /// </summary>
        public bool Connect()
        {
            // Publish Message
            _communicationController.PublishMarketAdminMessageResponse("DataLogin");

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("LoginPublished", _type.FullName, "Connect");
            }

            ConnectionStatus = true;
            return ConnectionStatus;
        }

        /// <summary>
        /// Ends Connection with Simulated Exchange
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            _communicationController.Disconnect();

            ConnectionStatus = false;
            return !ConnectionStatus;
        }

        /// <summary>
        /// Subscribe to New Symbol for bar data
        /// </summary>
        /// <param name="request"></param>
        public void SubscribeSymbol(BarDataRequest request)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Simulated Exchange Recieved Request for " + request, _type.FullName, "SubscribeSymbol");
            }

            // Add new symbol to the Bar list
            if (!_barSubscriptionList.Contains(request.Security.Symbol))
            {
                _barSubscriptionList.Add(request.Security.Symbol);
            }

            // Fetch data if its not already fetched for ticks
            if (!_tickSubscriptionList.Contains(request.Security.Symbol))
            {
                FetchData(request);
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
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("New subscription request received " + subscribe, _type.FullName, "SubscribeSymbol");
                }

                // Add new symbol to the Tick list
                if (!_tickSubscriptionList.Contains(subscribe.Security.Symbol))
                {
                    _tickSubscriptionList.Add(subscribe.Security.Symbol);
                }

                // Fetch data if its not already fetched for bars
                if(!_barSubscriptionList.Contains(subscribe.Security.Symbol))
                {
                    FetchData(subscribe);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeSymbol");
            }
        }

        /// <summary>
        /// Subscribe to New Symbol for Historical data
        /// </summary>
        /// <param name="request"></param>
        public void SubscribeSymbol(HistoricDataRequest request)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Simulated Exchange Recieves Request for " + request, _type.FullName, "SubscribeSymbol");
            }
            FetchData(request);
        }

        /// <summary>
        /// Hook All required Events
        /// </summary>
        private void SubscribeRequiredEvents()
        {
            try
            {
                // Listen to Login Requests
                _communicationController.MarketDataLoginRequest += MEDLoginArrived;

                // Listen to Market Data Requests
                _communicationController.TickDataRequest += MEDTickRequestArrived;
                _communicationController.BarDataRequest += MEDBarRequestArrived;
                _communicationController.HistoricDataRequest += MEDHistoricalRequestArrived;

            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeRequiredEvents");
            }
        }

        /// <summary>
        /// Event Fired When Request arrives
        /// </summary>
        /// <param name="barDataRequest"></param>
        private void MEDBarRequestArrived(BarDataRequest barDataRequest)
        {
            SubscribeSymbol(barDataRequest);
        }

        /// <summary>
        /// Event Fired When Tick Susbcription Request arrives
        /// </summary>
        /// <param name="subscribe"></param>
        private void MEDTickRequestArrived(Subscribe subscribe)
        {
            SubscribeSymbol(subscribe);
        }

        /// <summary>
        /// Event Fired When Historical Data Request arrives
        /// </summary>
        /// <param name="historicDataRequest"></param>
        private void MEDHistoricalRequestArrived(HistoricDataRequest historicDataRequest)
        {
            SubscribeSymbol(historicDataRequest);
        }

        /// <summary>
        /// Logon Arrived from 
        /// </summary>
        /// <param name="login"></param>
        private void MEDLoginArrived(Login login)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Login Requested From Market Data Engine", _type.FullName, "MEDLoginArrived");
                }
                if (login.MarketDataProvider==MarketDataProvider.SimulatedExchange)
                {
                    Connect();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "MEDLoginArrived");
            }
        }

        /// <summary>
        /// Logon Arrived from 
        /// </summary>
        private void MEDLoginArrived()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Login Requested From Market Data Engine", _type.FullName, "MEDLoginArrived");
                }

                Connect();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "MEDLoginArrived");
            }
        }

        /// <summary>
        /// Fire Historical Bars (Live Bar request)
        /// </summary>
        /// <param name="bar"></param>
        /// <param name="id"></param>
        private void FetchMarketDataBarFired(Bar bar, string id)
        {
            try
            {
                // Publish Bar if the subscription request is received
                if (_barSubscriptionList.Contains(bar.Security.Symbol))
                {
                    _communicationController.PublishBarData(bar);

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(bar.ToString(), _type.FullName, "FetchMarketDataBarFired");
                    }

                    // Notify Simulated Order Controller
                    // EventSystem.Publish<Bar>(bar);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "FetchMarketDataBarFired");
            }
        }

        /// <summary>
        /// Fire Historical Tick data
        /// </summary>
        /// <param name="tick">TradeHub Tick Containing required Info</param>
        private void FetchMarketDataTickFired(Tick tick)
        {
            try
            {
                // Publish Tick if the subscription request is received
                if (_tickSubscriptionList.Contains(tick.Security.Symbol))
                {
                    _communicationController.PublishTickData(tick);
                    
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(tick.ToString(), _type.FullName, "FetchMarketDataTickFired");
                    }
                }

                // Notify Simulated Order Controler
                EventSystem.Publish<Tick>(tick);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "FetchMarketDataTickFired");
            }
        }

        /// <summary>
        /// Fire Historical Bar Data
        /// </summary>
        /// <param name="historicBarData">TradeHub HistoricalBarData contains requested historical bars</param>
        private void FetchMarketDataHistoricDataFired(HistoricBarData historicBarData)
        {
            try
            {
                _communicationController.PublishHistoricData(historicBarData);
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(historicBarData.ToString(), _type.FullName, "FetchMarketDataHistoricDataFired");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "FetchMarketDataHistoricDataFired");
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
                Task.Factory.StartNew(() => FetchMarketData.ReadData(request));
                //FetchMarketData.ReadData(request);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "FetchData");
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
                Task.Factory.StartNew(() => FetchMarketData.ReadData(subscribe));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "FetchData");
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
                Task.Factory.StartNew(() => FetchMarketData.ReadData(historicDataRequest));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "FetchData");
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
                Logger.Error(exception, _type.FullName, "OnDataCompleted");
            }
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
                if (_tickSubscriptionList.Contains(data.Tick.Security.Symbol))
                {
                    _communicationController.PublishTickData(data.Tick);

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(data.Tick.ToString(), _type.FullName, "OnNext");
                    }
                }
            }
            else
            {
                // Publish Bar if the subscription request is received
                if (_barSubscriptionList.Contains(data.Bar.Security.Symbol))
                {
                    _communicationController.PublishBarData(data.Bar);

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(data.Bar.ToString(), _type.FullName, "OnNext");
                    }

                    // Notify Simulated Order Controller
                    // EventSystem.Publish<Bar>(bar);
                }
            }
        }

        #endregion
    }
}
