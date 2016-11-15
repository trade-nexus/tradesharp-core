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
using fxcore2;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.MarketDataProvider.Fxcm.Provider
{
    class HistoricalDataListener : IO2GResponseListener
    {
        private Type _type = typeof (HistoricalDataListener);

        /// <summary>
        /// Holds logger reference to calling class logger object
        /// </summary>
        private readonly AsyncClassLogger _logger;

        /// <summary>
        /// Holds all historic bars while waiting for completion
        /// </summary>
        private List<Bar> _barCollection;

        private O2GSession _session;
        private string _requestId;
        private O2GResponse _response;
        private EventWaitHandle _syncResponseEvent;

        #region Events

        // ReSharper Disable InconsistentNaming
        private event Action<HistoricBarData> _historicalDataEvent;
        // ReSharper Enable InconsistentNaming

        /// <summary>
        /// Event is raised to new Historical data is received
        /// </summary>
        public event Action<HistoricBarData> HistoricalDataEvent
        {
            add
            {
                if (_historicalDataEvent == null)
                {
                    _historicalDataEvent += value;
                }
            }
            remove { _historicalDataEvent -= value; }
        }

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="session"></param>
        /// <param name="logger"></param>
        public HistoricalDataListener(O2GSession session, AsyncClassLogger logger)
        {
            _session = session;
            _logger = logger;

            _requestId = string.Empty;
            _response = null;

            _syncResponseEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
            _barCollection = new List<Bar>();
        }

        public void SetRequestId(string requestId)
        {
            _response = null;
            _requestId = requestId;
        }

        public bool WaitEvents()
        {
            return _syncResponseEvent.WaitOne(30000);
        }

        public O2GResponse GetResponse()
        {
            return _response;
        }

        #region IO2GResponseListener Members

        public void onRequestCompleted(string sRequestId, O2GResponse response)
        {
            if (_requestId.Equals(response.RequestID))
            {
                _response = response;
                _syncResponseEvent.Set();
            }
        }

        public void onRequestFailed(string sRequestID, string sError)
        {
            if (_requestId.Equals(sRequestID))
            {
                _response = null;
                if (string.IsNullOrEmpty(sError))
                {
                    _logger.Error("There is no more data", _type.FullName, "onRequestFailed");
                }
                else
                {
                    _logger.Error("Request failed: " + sError, _type.FullName, "onRequestFailed");
                }
                _syncResponseEvent.Set();
            }
        }

        public void onTablesUpdates(O2GResponse data)
        {
        }

        #endregion

        /// <summary>
        /// Fetches historical data from FXCM server
        /// </summary>
        /// <param name="historicDataRequest"></param>
        public void GetHistoricalData(HistoricDataRequest historicDataRequest)
        {
            // Create a new factory
            O2GRequestFactory factory = _session.getRequestFactory();

            // Get time frame
            O2GTimeframe timeframe = factory.Timeframes[GetFxcmTimeFrame(historicDataRequest)];

            if (timeframe == null)
            {
                _logger.Error("Invalid time format", _type.FullName, "GetHistoricalData");

                return;
            }
            
            // Clear any existing data
            _barCollection.Clear();

            // Request historical snapshot
            O2GRequest request = factory.createMarketDataSnapshotRequestInstrument(historicDataRequest.Security.Symbol, timeframe, 300);

            DateTime endTime = historicDataRequest.EndTime;
            do // cause there is limit for returned candles amount
            {
                factory.fillMarketDataSnapshotRequestTime(request, historicDataRequest.StartTime, endTime, false);
                
                SetRequestId(request.RequestID);
                
                _session.sendRequest(request);

                if (!WaitEvents())
                {
                    _logger.Error("Response waiting timeout expired", _type.FullName, "GetHistoricalData");

                    return;
                }

                // Shift "to" bound to oldest datetime of returned data
                O2GResponse response = GetResponse();
                if (response != null && response.Type == O2GResponseType.MarketDataSnapshot)
                {
                    O2GResponseReaderFactory readerFactory = _session.getResponseReaderFactory();
                    if (readerFactory != null)
                    {
                        O2GMarketDataSnapshotResponseReader reader = readerFactory.createMarketDataSnapshotReader(response);
                        if (reader.Count > 0)
                        {
                            if (DateTime.Compare(endTime, reader.getDate(0)) != 0)
                            {
                                endTime = reader.getDate(0); // earliest datetime of returned data
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (_logger.IsInfoEnabled)
                            {
                                _logger.Info("Response waiting timeout expired", _type.FullName, "GetHistoricalData");
                            }
                            break;
                        }
                    }
                    ExtractPrices(_session, response, historicDataRequest);
                }
                else
                {
                    break;
                }
            } while (endTime > historicDataRequest.StartTime);

            if (_barCollection.Count > 0)
            {
                if (_historicalDataEvent != null)
                {
                    _barCollection.Reverse();
                    
                    // Create historical data object
                    var historicalData = new HistoricBarData(historicDataRequest.Security, historicDataRequest.MarketDataProvider, DateTime.UtcNow);
                    historicalData.Bars = _barCollection.ToArray();
                    historicalData.ReqId = historicDataRequest.Id;

                    // Raise Event
                    _historicalDataEvent(historicalData);
                }

                _barCollection.Clear();
            }
        }

        /// <summary>
        /// Print history data from response
        /// </summary>
        /// <param name="session"></param>
        /// <param name="response"></param>
        /// <param name="request"></param>
        private void ExtractPrices(O2GSession session, O2GResponse response, HistoricDataRequest request)
        {
            O2GResponseReaderFactory factory = session.getResponseReaderFactory();
            if (factory != null)
            {
                O2GMarketDataSnapshotResponseReader reader = factory.createMarketDataSnapshotReader(response);
                for (int decrement = reader.Count - 1; decrement >= 0; decrement--)
                {
                    if (reader.isBar)
                    {
                        // Extract Time
                        var dateTime = reader.getDate(decrement);

                        // Create new Bar object
                        Bar bar = new Bar(request.Security, request.MarketDataProvider, request.Id, dateTime);

                        // Set bar fields
                        bar.High = Convert.ToDecimal(reader.getBidHigh(decrement));
                        bar.Low = Convert.ToDecimal(reader.getBidLow(decrement));
                        bar.Open = Convert.ToDecimal(reader.getBidOpen(decrement));
                        bar.Close = Convert.ToDecimal(reader.getBidClose(decrement));

                        bar.Volume = reader.getVolume(decrement);

                        // Add to local collection
                        _barCollection.Add(bar);
                    }
                }
            }
        }

        /// <summary>
        /// Returns time frame in FXCM format
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private string GetFxcmTimeFrame(HistoricDataRequest request)
        {
            string timeFrame = "m1";

            if (request.BarType.Equals(BarType.INTRADAY))
            {
                if (request.Interval.Equals(1))
                {
                    timeFrame = "m1";
                }
                else if (request.Interval.Equals(15))
                {
                    timeFrame = "m15";
                }
                else if (request.Interval.Equals(30))
                {
                    timeFrame = "m30";
                }
                else if (request.Interval.Equals(1*60))
                {
                    timeFrame = "H1";
                }
                else if (request.Interval.Equals(2 * 60))
                {
                    timeFrame = "H2";
                }
                else if (request.Interval.Equals(3 * 60))
                {
                    timeFrame = "H3";
                }
                else if (request.Interval.Equals(4 * 60))
                {
                    timeFrame = "H4";
                }
                else if (request.Interval.Equals(6 * 60))
                {
                    timeFrame = "H6";
                }
                else if (request.Interval.Equals(8 * 60))
                {
                    timeFrame = "H8";
                }
            }
            else if(request.BarType.Equals(BarType.DAILY))
            {
                timeFrame = "D1";
            }
            else if (request.BarType.Equals(BarType.WEEKLY))
            {
                timeFrame = "W1";
            }
            else if (request.BarType.Equals(BarType.MONTHLY))
            {
                timeFrame = "M1";
            }

            return timeFrame;
        }
    }
}
