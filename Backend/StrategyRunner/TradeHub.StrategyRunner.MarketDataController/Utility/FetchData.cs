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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Disruptor;
using Disruptor.Dsl;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.StrategyRunner.Infrastructure.ValueObjects;
using TradeHub.StrategyRunner.MarketDataController.Service;

namespace TradeHub.StrategyRunner.MarketDataController.Utility
{
    /// <summary>
    /// Responsible for getting the required data 
    /// </summary>
    public class FetchData
    {
        private Type _type = typeof(FetchData);
        private readonly AsyncClassLogger _classLogger;

        //public event Action<Bar, string> BarFired;
        //public event Action<Tick> TickFired;
        public event Action<HistoricBarData> HistoricalDataFired;

        private ReadMarketData _readMarketData;
        private DateTime _startDate = new DateTime(2013, 08, 06);
        private DateTime _endDate = new DateTime(2013, 08, 07);
        private string _providerName = MarketDataProvider.InteractiveBrokers;

        private readonly int _ringSize = 1048576;//262144;//65536;  // Must be multiple of 2

        private Disruptor<MarketDataObject> _disruptor;
        private RingBuffer<MarketDataObject> _ringBuffer;
        private EventPublisher<MarketDataObject> _publisher;

        /// <summary>
        /// Constructor:
        /// Loads Setting From Xml File
        /// </summary>
        /// <param name="readMarketData"></param>
        /// <param name="classLogger"> </param>
        public FetchData(ReadMarketData readMarketData, AsyncClassLogger classLogger)
        {
            try
            {
                _classLogger = classLogger;
                _readMarketData = readMarketData;

                //Reading Settings From Xml File
                string path = "MarketDataConfiguration\\MarketDataController.xml";
                XDocument doc;
                if (File.Exists(path))
                {
                    _classLogger.Info("1st", _type.FullName, "FetchData");
                    doc = XDocument.Load("MarketDataConfiguration\\MarketDataController.xml");
                    
                }
                else
                {
                    _classLogger.Info("2nd", _type.FullName, "FetchData");
                    doc = XDocument.Load("..\\Strategy Runner\\MarketDataConfiguration\\MarketDataController.xml");   
                }
                var startDate = doc.Descendants("StartDate");
                foreach (var xElement in startDate)
                {
                    string[] start = xElement.Value.Split(',');
                    _startDate = new DateTime(Convert.ToInt32(start[0]), Convert.ToInt32(start[1]), Convert.ToInt32(start[2]));
                    if (_classLogger.IsInfoEnabled)
                    {
                        _classLogger.Info("StartDate:" + _startDate.ToString(CultureInfo.InvariantCulture), _type.FullName, "FetchData");
                    }
                }

                var endDate = doc.Descendants("EndDate");
                foreach (var xElement in endDate)
                {
                    string[] end = xElement.Value.Split(',');
                    _endDate = new DateTime(Convert.ToInt32(end[0]), Convert.ToInt32(end[1]), Convert.ToInt32(end[2]));
                    if (_classLogger.IsInfoEnabled)
                    {
                        _classLogger.Info("EndDate:" + _endDate.ToString(CultureInfo.InvariantCulture), _type.FullName, "FetchData");
                    }
                }

                var provider = doc.Descendants("Provider");
                foreach (var xElement in provider)
                {
                    _providerName = xElement.Value;
                    if (_classLogger.IsInfoEnabled)
                    {
                        _classLogger.Info("ProviderName:" + _providerName.ToString(CultureInfo.InvariantCulture), _type.FullName, "FetchData");
                    }
                }
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "FetchData");
            }
        }

        /// <summary>
        /// Reading Data From ReadMarketData class.
        /// </summary>
        /// <param name="request"></param>
        public virtual void ReadData(BarDataRequest request)
        {
            try
            {
                IEnumerable<Bar> barlist = _readMarketData.ReadBars(_startDate, _endDate, _providerName, request);

                #region Send Required Info

                foreach (var bar in barlist)
                {
                    // Update time
                    DateTime time = bar.DateTime.AddMinutes(-1);

                    for (int i = 0; i < 4; i++)
                    {
                        // Raise event to notify listeners
                        _publisher.PublishEvent((entry, sequenceNo) =>
                            {
                                // Update Security value
                                entry.Tick.Security = new Security() {Symbol = bar.Security.Symbol};
                                // Update Market Data Provider value
                                entry.Tick.MarketDataProvider = MarketDataProvider.SimulatedExchange;
                                // Update Last Price Value
                                entry.Tick.LastPrice = GetRequiredPrice(i, bar);
                                // Update Size
                                entry.Tick.LastSize = 100;
                                // Set updated time
                                entry.Tick.DateTime = time.AddSeconds((i + 1)*14);

                                entry.IsTick = true;
                                return entry;
                            });
                    }

                    // Raise event to notify listeners
                    _publisher.PublishEvent((entry, sequenceNo) =>
                        {
                            entry.Bar.Open = bar.Open;
                            entry.Bar.High = bar.High;
                            entry.Bar.Low = bar.Low;
                            entry.Bar.Close = bar.Close;

                            entry.Bar.RequestId = bar.RequestId;
                            entry.Bar.DateTime = bar.DateTime;
                            entry.Bar.MarketDataProvider = MarketDataProvider.SimulatedExchange;
                            entry.Bar.Security = new Security() {Symbol = bar.Security.Symbol};

                            return entry;
                        });
                }

                #endregion
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "ReadData - BarDataRequest");
            }
        }

        /// <summary>
        /// Reads data for required symbol from stored files
        /// </summary>
        /// <param name="subscribe">Contains Symbol info</param>
        public virtual void ReadData(Subscribe subscribe)
        {
            try
            {
                // Get all available Bars
                var barlist = _readMarketData.ReadBars(_startDate, _endDate, _providerName, subscribe);

                #region Send Required Info

                foreach (var bar in barlist)
                {
                    // Update time
                    DateTime time = bar.DateTime.AddMinutes(-1);

                    for (int i = 0; i < 4; i++)
                    {
                        //Create Object to be disptched
                        MarketDataObject marketDataObjectTick = new MarketDataObject();

                        // Create a new tick object
                        Tick tick = new Tick(bar.Security, MarketDataProvider.SimulatedExchange)
                        {
                            // Add Last Price to new Tick instance
                            LastPrice = GetRequiredPrice(i, bar),
                            // Add Size
                            LastSize = 100,
                            // Set updated time
                            DateTime = time.AddSeconds((i + 1) * 14)
                        };

                        // Add Values to the object to be dispatched
                        marketDataObjectTick.IsTick = true;
                        marketDataObjectTick.Tick = tick;

                        // Raise event to notify listeners
                        _publisher.PublishEvent((entry, sequenceNo) =>
                        {
                            entry.IsTick = marketDataObjectTick.IsTick;
                            entry.Tick = marketDataObjectTick.Tick;
                            return entry;
                        });
                    }

                    //Create Object to be disptched
                    MarketDataObject marketDataObjectBar = new MarketDataObject();
                    marketDataObjectBar.Bar = bar;
                    //TODO: Valid request ID needs to be added
                    marketDataObjectBar.Bar.RequestId = "";

                    _publisher.PublishEvent((entry, sequenceNo) =>
                    {
                        entry.Bar = marketDataObjectBar.Bar;
                        return entry;
                    });
                }

                #endregion
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "ReadData - Subscribe");
            }
        }

        /// <summary>
        /// Reads data for required symbol from stored files
        /// </summary>
        /// <param name="historicDataRequest">Contains historical request info for subscribing symbol</param>
        public virtual void ReadData(HistoricDataRequest historicDataRequest)
        {
            try
            {
                // Get all available Bars
                var barlist = _readMarketData.ReadBars(_providerName, historicDataRequest);

                // Create new historic bar data response object
                HistoricBarData historicBarData = new HistoricBarData(historicDataRequest.Security, MarketDataProvider.SimulatedExchange, DateTime.Now);

                // Add Bars
                historicBarData.Bars = barlist.ToArray();
                // Add Request ID
                historicBarData.ReqId = historicDataRequest.Id;

                // Raise event to notify listeners
                if (HistoricalDataFired != null)
                {
                    HistoricalDataFired(historicBarData);
                }
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "ReadData - HistoricDataRequest");
            }
        }

        /// <summary>
        /// Initialize Disruptor with appropriate event handler
        /// </summary>
        /// <param name="eventHandler">Event Handler which will receive data from disruptor</param>
        public void InitializeDisruptor(IEventHandler<MarketDataObject>[] eventHandler)
        {
            if (_disruptor == null)
            {
                // Initialize Disruptor
                _disruptor = new Disruptor<MarketDataObject>(() => new MarketDataObject(), _ringSize, TaskScheduler.Default);
            }
            else
            {
                // Shutdown disruptor if it was already running
                _disruptor.Shutdown();
            }

            // Add Consumer
            _disruptor.HandleEventsWith(eventHandler);
            // Start Disruptor
            _ringBuffer = _disruptor.Start();
            // Get Publisher
            _publisher = new EventPublisher<MarketDataObject>(_ringBuffer);
        }

        /// <summary>
        /// Provides required price type from the given bar
        /// </summary>
        /// <param name="iteration">Iteration count</param>
        /// <param name="bar">Bar to process</param>
        /// <returns></returns>
        private decimal GetRequiredPrice(int iteration, Bar bar)
        {
            switch (iteration)
            {
                case 0:
                    return bar.Open;
                case 1:
                    return bar.High;
                case 2:
                    return bar.Low;
                case 3:
                    return bar.Close;
                default:
                    return default(decimal);
            }
        }


        /// <summary>
        /// Shutsdown the disruptor used
        /// </summary>
        public void ShutdownDisruptor()
        {
            if (_disruptor != null)
            {
                // Shutdown disruptor if it was already running
                _disruptor.Shutdown();
            }
        }

    }
}
