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
using System.ComponentModel;
using System.Windows.Threading;
using TradeHub.Common.Core.DomainModels;
using TradeSharp.UI.Common.Constants;
using TradeSharp.UI.Common.Utility;
using TradeSharp.UI.Common.ValueObjects;

namespace TradeSharp.UI.Common.Models
{
    /// <summary>
    /// Contains necessary Market data information to be displayed on the UI for given Symbol
    /// </summary>
    public class MarketDataDetail : INotifyPropertyChanged
    {
        /// <summary>
        /// Holds UI thread reference
        /// </summary>
        private Dispatcher _currentDispatcher;

        /// <summary>
        /// Contains Tick's Symbol information
        /// </summary>
        private Security _security;

        /// <summary>
        /// Best available Bid price
        /// </summary>
        private decimal _bidPrice;

        /// <summary>
        /// Best avaiable Ask Price
        /// </summary>
        private decimal _askPrice;

        /// <summary>
        /// Price for Last Traded 
        /// </summary>
        private decimal _lastPrice;

        /// <summary>
        /// Quantity corresponding to Best Bid
        /// </summary>
        private decimal _bidQuantity;

        /// <summary>
        /// Quantity corresponding to Best Ask
        /// </summary>
        private decimal _askQuantity;

        /// <summary>
        /// Last Traded quantity
        /// </summary>
        private decimal _lastQuantity;

        /// <summary>
        /// Contains information about which type of data to be persisted
        /// </summary>
        private DataPersistenceInformation _persistenceInformation;

        /// <summary>
        /// Responsible for creating Price Alerts based on specified conditions
        /// </summary>
        private PriceAlertGenerator _priceAlertGenerator;

        /// <summary>
        /// Contains the maping of Bid LOB-Record for each depth
        /// KEY = Depth
        /// VALUE = Limit Order Book Record for Bid <see cref="LimitOrderBookRecord"/>
        /// </summary>
        private Dictionary<int, LimitOrderBookRecord> _bidRecordsMap;

        /// <summary>
        /// Contains the maping of Ask LOB-Record for each depth
        /// KEY = Depth
        /// VALUE = Limit Order Book Record for Ask <see cref="LimitOrderBookRecord"/>
        /// </summary>
        private Dictionary<int, LimitOrderBookRecord> _askRecordsMap;

        /// <summary>
        /// Contains Limit order book entries for BIDS
        /// </summary>
        private SortedObservableCollection<LimitOrderBookRecord> _bidRecordsCollection;

        /// <summary>
        /// Contains Limit order book entries for ASKS
        /// </summary>
        private SortedObservableCollection<LimitOrderBookRecord> _askRecordsCollection;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="security">Contains symbol information</param>
        public MarketDataDetail(Security security)
        {
            // Save UI thread reference
            _currentDispatcher = Dispatcher.CurrentDispatcher;

            // Save reference
            _security = security;

            // Initialize parameters
            _priceAlertGenerator = new PriceAlertGenerator();
            _persistenceInformation = new DataPersistenceInformation();

            _bidRecordsMap = new Dictionary<int, LimitOrderBookRecord>();
            _askRecordsMap = new Dictionary<int, LimitOrderBookRecord>();

            _bidRecordsCollection = new SortedObservableCollection<LimitOrderBookRecord>();
            _askRecordsCollection = new SortedObservableCollection<LimitOrderBookRecord>();
        }

        #region Properties

        /// <summary>
        /// Contains Tick's Symbol information
        /// </summary>
        public Security Security
        {
            get { return _security; }
            set { _security = value; }
        }

        /// <summary>
        /// Best available Bid price
        /// </summary>
        public decimal BidPrice
        {
            get { return _bidPrice; }
            set
            {
                _bidPrice = value;
                OnPropertyChanged("BidPrice");
            }
        }

        /// <summary>
        /// Best avaiable Ask Price
        /// </summary>
        public decimal AskPrice
        {
            get { return _askPrice; }
            set
            {
                _askPrice = value;
                OnPropertyChanged("AskPrice");
            }
        }

        /// <summary>
        /// Price for Last Traded 
        /// </summary>
        public decimal LastPrice
        {
            get { return _lastPrice; }
            set
            {
                _lastPrice = value;
                OnPropertyChanged("LastPrice");
            }
        }

        /// <summary>
        /// Quantity corresponding to Best Bid
        /// </summary>
        public decimal BidQuantity
        {
            get { return _bidQuantity; }
            set
            {
                _bidQuantity = value;
                OnPropertyChanged("BidQuantity");
            }
        }

        /// <summary>
        /// Quantity corresponding to Best Ask
        /// </summary>
        public decimal AskQuantity
        {
            get { return _askQuantity; }
            set
            {
                _askQuantity = value;
                OnPropertyChanged("AskQuantity");
            }
        }

        /// <summary>
        /// Last Traded quantity
        /// </summary>
        public decimal LastQuantity
        {
            get { return _lastQuantity; }
            set
            {
                _lastQuantity = value;
                OnPropertyChanged("LastQuantity");
            }
        }

        /// <summary>
        /// Contains all Limit order book entries for BIDs
        /// </summary>
        public SortedObservableCollection<LimitOrderBookRecord> BidRecordsCollection
        {
            get { return _bidRecordsCollection; }
            set
            {
                _bidRecordsCollection = value;
                OnPropertyChanged("BidRecordsCollection");
            }
        }

        /// <summary>
        /// Contains all Limit order book entries for ASKs
        /// </summary>
        public SortedObservableCollection<LimitOrderBookRecord> AskRecordsCollection
        {
            get { return _askRecordsCollection; }
            set
            {
                _askRecordsCollection = value;
                OnPropertyChanged("AskRecordsCollection");
            }
        }

        /// <summary>
        /// Contains information about which type of data to be persisted
        /// </summary>
        public DataPersistenceInformation PersistenceInformation
        {
            get { return _persistenceInformation; }
        }

        #endregion

        /// <summary>
        /// Updates best market values and Bid/Ask collections for depth management
        /// </summary>
        /// <param name="tick"></param>
        public void Update(Tick tick)
        {
            // Update bids collection
            if (tick.HasBid)
            {
                LimitOrderBookRecord bidRecord;
                if (!_bidRecordsMap.TryGetValue(tick.Depth, out bidRecord))
                {
                    bidRecord = new LimitOrderBookRecord(LobRecordType.Bid);

                    // Add new value to Map
                    _bidRecordsMap.Add(tick.Depth, bidRecord);

                    _currentDispatcher.Invoke(DispatcherPriority.Background, (Action)(() =>
                    {
                        // Add new value to Collection
                        BidRecordsCollection.Add(bidRecord);
                    }));
                }

                _currentDispatcher.Invoke(DispatcherPriority.Background, (Action)(() =>
                {
                    if (tick.Depth == 0)
                    {
                        // Update Bid
                        BidPrice = tick.BidPrice;
                        BidQuantity = tick.BidSize;
                    }

                    // TODO: try to optimize this
                    BidRecordsCollection.Remove(bidRecord);
                   
                    // Update values
                    bidRecord.Depth = tick.Depth;
                    bidRecord.BidPrice = tick.BidPrice;
                    bidRecord.BidQuantity = tick.BidSize;

                    BidRecordsCollection.Add(bidRecord);
                }));
            }

            // Update asks collection
            if (tick.HasAsk)
            {
                LimitOrderBookRecord askRecord;
                if (!_askRecordsMap.TryGetValue(tick.Depth, out askRecord))
                {
                    askRecord = new LimitOrderBookRecord(LobRecordType.Ask);

                    // Add new value to Map
                    _askRecordsMap.Add(tick.Depth, askRecord);

                    _currentDispatcher.Invoke(DispatcherPriority.Background, (Action)(() =>
                    {
                        // Add new value to Collection
                        AskRecordsCollection.Add(askRecord);
                    }));
                }

                _currentDispatcher.Invoke(DispatcherPriority.Background, (Action)(() =>
                {
                    if (tick.Depth == 0)
                    {
                        // Update Ask
                        AskPrice = tick.AskPrice;
                        AskQuantity = tick.AskSize;
                    }

                    // TODO: try to optimize this
                    AskRecordsCollection.Remove(askRecord);
                    
                    // Update values
                    askRecord.Depth = tick.Depth;
                    askRecord.AskPrice = tick.AskPrice;
                    askRecord.AskQuantity = tick.AskSize;

                    AskRecordsCollection.Add(askRecord);
                }));
            }

            if (tick.HasTrade)
            {
                _currentDispatcher.Invoke(DispatcherPriority.Background, (Action)(() =>
                {
                    // Update Last
                    LastPrice = tick.LastPrice;
                    LastQuantity = tick.LastSize;
                }));
            }

            _priceAlertGenerator.EvaluateConditions(tick);
        }

        /// <summary>
        /// Adds new alert conditions for BIDS
        /// </summary>
        public void AddBidAlertConditions(List<PriceAlertCondition> alertConditions)
        {
            _priceAlertGenerator.AddBidAlertConditions(alertConditions);
        }

        /// <summary>
        /// Adds new alert conditions for ASKS
        /// </summary>
        public void AddAskAlertConditions(List<PriceAlertCondition> alertConditions)
        {
            _priceAlertGenerator.AddAskAlertConditions(alertConditions);
        }

        /// <summary>
        /// Adds new alert conditions for TRADES
        /// </summary>
        public void AddTradeAlertConditions(List<PriceAlertCondition> alertConditions)
        {
            _priceAlertGenerator.AddTradeAlertConditions(alertConditions);
        }

        /// <summary>
        /// Returns existing BID conditions
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<PriceAlertCondition> GetBidAlertConditions()
        {
            return _priceAlertGenerator.BidPriceConditions;
        }

        /// <summary>
        /// Returns existing ASK conditions
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<PriceAlertCondition> GetAskAlertConditions()
        {
            return _priceAlertGenerator.AskPriceConditions;
        }

        /// <summary>
        /// Returns existing TRADE conditions
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<PriceAlertCondition> GetTradeAlertConditions()
        {
            return _priceAlertGenerator.TradePriceConditions;
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
