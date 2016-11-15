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
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.MarketDataProvider.IqFeed.Provider
{
    /// <summary>
    /// Responsible for providing Historical data feed
    /// </summary>
    internal class HistoricalData
    {
        private Type _type = typeof(BarData);

        /// <summary>
        /// Holds the logger instance of the calling class
        /// </summary>
        private AsyncClassLogger _logger;

        /// <summary>
        /// Contains Provider name used through out TradeSharp
        /// </summary>
        private readonly string _marketDataProviderName;
        
        /// <summary>
        /// Holds all historic bars while waiting for completion
        /// </summary>
        private List<Bar> _barCollection;

        /// <summary>
        /// Contains map of symbols of each requested ID
        /// KEY = Request ID
        /// VALUE = Symbol
        /// </summary>
        private Dictionary<string, string> _requestIdToSymbolsMap; 

        /// <summary>
        /// Responsible for communicating with IQ Feed Connector
        /// </summary>
        private DTNIQFeedCOMLib.HistoryLookup4 _historyLookupComObject;

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
        /// <param name="logger">logger instance of the calling class</param>
        public HistoricalData(AsyncClassLogger logger)
        {
            // Save instance
            _logger = logger;

            // Set provider name to be used in all calls
            _marketDataProviderName = Constants.MarketDataProvider.IqFeed;

            // Initialize
            _barCollection = new List<Bar>();
            _requestIdToSymbolsMap = new Dictionary<string, string>();
        }

        /// <summary>
        /// Opens a connection to the IQFeed Connector
        /// </summary>
        public void OpenHistoricalDataConnection()
        {
            try
            {
                // Initialize IQFeed COM object
                _historyLookupComObject = new DTNIQFeedCOMLib.HistoryLookup4();

                // Hook necessary IQ Feed events
                RegisterEvents();

                // Use request to initiate the connection and set our IQFeed protocol here.
                _historyLookupComObject.SetProtocol("5.1");
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "OpenBarDataConnection");
            }
        }

        /// <summary>
        /// Subscribes to necessary IQ Feed events for Level 1 Data
        /// </summary>
        private void RegisterEvents()
        {
            // Makes sure that events are subscribed only once
            UnregisterEvents();

            // Add handlers for the various message types
            _historyLookupComObject.OnIntervalMsg += OnIntervalMessage;
            _historyLookupComObject.OnDWMMsg += OnDailyWeeklyMonthlyMessage;
            _historyLookupComObject.OnErrorMsg += OnErrorMessage;
        }

        /// <summary>
        /// Un-Subscribes existing IQ Feed events for Level 1 Data
        /// </summary>
        private void UnregisterEvents()
        {
            // Add handlers for the various message types
            _historyLookupComObject.OnIntervalMsg -= OnIntervalMessage;
            _historyLookupComObject.OnDWMMsg -= OnDailyWeeklyMonthlyMessage;
            _historyLookupComObject.OnErrorMsg -= OnErrorMessage;
        }

        /// <summary>
        /// Sends subcription request to IQ Feed Connnector
        /// </summary>
        /// <param name="historicDataRequest"></param>
        public void Subscribe(HistoricDataRequest historicDataRequest)
        {
            // Interval Timeframe
            if (historicDataRequest.BarType.Equals(Constants.BarType.INTRADAY))
            {
                // Set interval type to Time
                string intervalType = "t";

                // Set interval
                int interval = (int) historicDataRequest.Interval;

                // Create time format string 
                string timeFormat = "yyyyMMdd hhmmss";

                // Get time intervals
                string beginDateTime = historicDataRequest.StartTime.ToString(timeFormat);
                string endDateTime = historicDataRequest.EndTime.ToString(timeFormat);

                // Add Request ID and Symbol to local map
                _requestIdToSymbolsMap.Add(historicDataRequest.Id, historicDataRequest.Security.Symbol);

                // Send request
                _historyLookupComObject.ReqHistoryIntervalTimeframe(historicDataRequest.Security.Symbol.ToUpperInvariant(),
                    interval, beginDateTime, endDateTime, 0, "", "", 0, historicDataRequest.Id, 0, intervalType);
            }
            else if (historicDataRequest.BarType.Equals(Constants.BarType.DAILY))
            {
                // Send request
                _historyLookupComObject.ReqHistoryDailyDatapoints(
                    historicDataRequest.Security.Symbol.ToUpperInvariant(), 0, 0, historicDataRequest.Id, 0);
            }
            else if (historicDataRequest.BarType.Equals(Constants.BarType.WEEKLY))
            {
                // Send request
                _historyLookupComObject.ReqHistoryWeeklyDatapoints(
                    historicDataRequest.Security.Symbol.ToUpperInvariant(), 0, 0, historicDataRequest.Id, 0);
            }
            else if (historicDataRequest.BarType.Equals(Constants.BarType.MONTHLY))
            {
                // Send request
                _historyLookupComObject.ReqHistoryMonthlyDatapoints(
                    historicDataRequest.Security.Symbol.ToUpperInvariant(), 0, 0, historicDataRequest.Id, 0);
            }
        }

        /// <summary>
        /// Called when new interval message is received
        /// </summary>
        /// <param name="receivedBarData"></param>
        private void OnIntervalMessage(ref string receivedBarData)
        {
            // Process and extract individual bars
            ProcessHistoricalData(receivedBarData);
        }

        /// <summary>
        /// Called when new Daily Weekly Monthly Message is received
        /// </summary>
        /// <param name="receivedBarData"></param>
        private void OnDailyWeeklyMonthlyMessage(ref string receivedBarData)
        {
            // Process and extract individual bars
            ProcessHistoricalData(receivedBarData);
        }

        /// <summary>
        /// Process incoming historical data to extract information
        /// </summary>
        /// <param name="receivedBarData"></param>
        private void ProcessHistoricalData(string receivedBarData)
        {
            using (StringReader reader = new StringReader(receivedBarData))
            {
                string singleLine;
                while ((singleLine = reader.ReadLine()) != null)
                {
                    if (singleLine.Contains("ENDMSG"))
                    {
                        // Notify listeners
                        if (_barCollection.Count > 0)
                        {
                            if (_historicalDataEvent != null)
                            {
                                // Create historical data object
                                var historicalData = new HistoricBarData(_barCollection[0].Security, _marketDataProviderName, DateTime.UtcNow);
                                historicalData.Bars = _barCollection.ToArray();
                                historicalData.ReqId = _barCollection[0].RequestId;

                                // Raise Event
                                _historicalDataEvent(historicalData);
                            }
                        }

                        // Remove ID to Symbol mapping of received data
                        _requestIdToSymbolsMap.Remove(_barCollection[0].RequestId);

                        // Clear collection for future use
                        _barCollection.Clear();

                    }
                    else
                    {
                        // Convert into TradeSharp Bar message
                        var bar = ParseHistoricalBarData(singleLine);

                        // Add to collection
                        if (bar != null)
                        {
                            _barCollection.Add(bar);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when new error is received
        /// </summary>
        /// <param name="errorMessage"></param>
        private void OnErrorMessage(ref string errorMessage)
        {
            _logger.Error(errorMessage, _type.FullName, "OnErrorMessage");
        }

        /// <summary>
        /// Converts the incoming data string to TradeSharp Bar message
        /// </summary>
        /// <param name="historicalData"></param>
        /// <returns></returns>
        private Bar ParseHistoricalBarData(string historicalData)
        {
            try
            {
                // Split incoming data
                string[] historicalDataArray = historicalData.Split(',');

                // Select time format string
                string timeFormat = historicalDataArray[1].Length.Equals(10) ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm:ss";
                
                // Get Request ID value
                var requestId = historicalDataArray[0];

                string symbol;
                _requestIdToSymbolsMap.TryGetValue(requestId, out symbol);

                // Create Security
                var security = new Security(){Symbol = symbol};

                // Extract Time
                var dateTime = DateTime.ParseExact(historicalDataArray[1], timeFormat, CultureInfo.InvariantCulture);

                // Create new Bar object
                Bar bar = new Bar(security, _marketDataProviderName, requestId, dateTime);

                // Set bar fields
                bar.High = Convert.ToDecimal(historicalDataArray[2]);
                bar.Low = Convert.ToDecimal(historicalDataArray[3]);
                bar.Open = Convert.ToDecimal(historicalDataArray[4]);
                bar.Close = Convert.ToDecimal(historicalDataArray[5]);

                bar.Volume = Convert.ToInt64(historicalDataArray[historicalDataArray.Length - 2]);

                // Return newly created bar object
                return bar;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "ParseHistoricalBarData");
                return null;
            }
        }
    }
}
