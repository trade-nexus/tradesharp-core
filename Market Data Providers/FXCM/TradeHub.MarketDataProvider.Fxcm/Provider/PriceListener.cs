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
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using fxcore2;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.MarketDataProvider.Fxcm.Provider
{
    class PriceListener : IO2GTableListener
    {
        private Type _type = typeof (PriceListener);

        /// <summary>
        /// Keeps track of subscribed symbols
        /// </summary>
        private List<string> _subscriptionList;

        /// <summary>
        /// Provides market data feed updates
        /// </summary>
        private O2GTableManager _tableManager;

        /// <summary>
        /// Holds the logger instance of the calling class
        /// </summary>
        private readonly AsyncClassLogger _logger;

        #region Events

        // ReSharper Disable InconsistentNaming
        private event Action<Tick> _dataEvent;
        // ReSharper Enable InconsistentNaming

        /// <summary>
        /// Event is raised to new Tick data is received
        /// </summary>
        public event Action<Tick> DataEvent
        {
            add
            {
                if (_dataEvent == null)
                {
                    _dataEvent += value;
                }
            }
            remove { _dataEvent -= value; }
        }

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public PriceListener(O2GTableManager tableManager, AsyncClassLogger logger)
        {
            // Save reference 
            _logger = logger;
            _tableManager = tableManager;

            // Initialize
            _subscriptionList = new List<string>();
        }

        /// <summary>
        /// Subscribe symbols price updates
        /// </summary>
        /// <param name="symbol"></param>
        public void Subscribe(string symbol)
        {
            if (_subscriptionList.Count==0)
            {
                O2GTableManagerStatus managerStatus = _tableManager.getStatus();
                while (managerStatus == O2GTableManagerStatus.TablesLoading)
                {
                    Thread.Sleep(50);
                    managerStatus = _tableManager.getStatus();
                }

                if (managerStatus == O2GTableManagerStatus.TablesLoadFailed)
                {
                    _logger.Error("Table loading failed", _type.FullName, "Subscribe");

                    return;
                }

                SubscribeEvents(_tableManager);
            }

            _subscriptionList.Add(symbol);
        }

        /// <summary>
        /// Un-Subscribe price updates
        /// </summary>
        /// <param name="symbol"></param>
        public void Unsubscribe(string symbol)
        {
            _subscriptionList.Remove(symbol);

            if (_subscriptionList.Count == 0)
            {
                UnsubscribeEvents(_tableManager);
            }
        }

        /// <summary>
        /// Un-Subscribe all price updates
        /// </summary>
        public void UnsubscribeAll()
        {
            if (_tableManager != null)
            {
                _subscriptionList.Clear();

                UnsubscribeEvents(_tableManager);
            }
        }

        #region IO2GTableListener Members

        // Implementation of IO2GTableListener interface public method onAdded
        public void onAdded(string sRowID, O2GRow rowData)
        {
        }

        // Implementation of IO2GTableListener interface public method onChanged
        public void onChanged(string sRowID, O2GRow rowData)
        {
            if (rowData.TableType == O2GTableType.Offers)
            {
                ExtractOfferDetails((O2GOfferTableRow)rowData);
            }
        }

        // Implementation of IO2GTableListener interface public method onDeleted
        public void onDeleted(string sRowID, O2GRow rowData)
        {
        }

        public void onStatusChanged(O2GTableStatus status)
        {
        }

        #endregion

        private void SubscribeEvents(O2GTableManager manager)
        {
            O2GOffersTable offersTable = (O2GOffersTable)manager.getTable(O2GTableType.Offers);
            offersTable.subscribeUpdate(O2GTableUpdateType.Update, this);
        }

        private void UnsubscribeEvents(O2GTableManager manager)
        {
            O2GOffersTable offersTable = (O2GOffersTable)manager.getTable(O2GTableType.Offers);
            offersTable.unsubscribeUpdate(O2GTableUpdateType.Update, this);
        }

        /// <summary>
        /// Extracts offer detials and creates TradeSharp Tick messages
        /// </summary>
        /// <param name="offerRow"></param>
        public void ExtractOfferDetails(O2GOfferTableRow offerRow)
        {
            try
            {
                if (_subscriptionList.Contains(offerRow.Instrument))
                {
                    // Create new Tick object
                    Tick tick = new Tick(new Security() {Symbol = offerRow.Instrument},
                        Common.Core.Constants.MarketDataProvider.Fxcm, offerRow.Time);

                    // Extract BID information
                    tick.BidPrice = (decimal) offerRow.Bid;
                    tick.BidSize = 1;

                    // Extract ASK information
                    tick.AskPrice = (decimal) offerRow.Ask;
                    tick.AskSize = 1;

                    // Raise event to notify listeners
                    if (_dataEvent != null)
                    {
                        _dataEvent(tick);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "ExtractOfferDetails");
            }
        }
    }
}
